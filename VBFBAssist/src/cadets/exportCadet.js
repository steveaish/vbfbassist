import * as XLSX from 'xlsx';
import JSZip from 'jszip';
import { saveAs } from 'file-saver';
import { ApiFileUrl } from '../utils/ApiUtils';
import { formatDate, yesNo } from '../utils/GridUtils';

// Build a single-row workbook for the Details tab
function buildDetailsSheet(form, crews) {
  const crewName = form.CrewName
    ?? crews.find((c) => c.seqno == form.cadetcrew)?.CrewName
    ?? '';
  const rows = [
    ['Field', 'Value'],
    ['First Name', form.firstname ?? ''],
    ['Last Name', form.lastname ?? ''],
    ['Email', form.email ?? ''],
    ['Crew', crewName],
    ['Caregiver Name', form.caregivername ?? ''],
    ['Caregiver Mobile', form.caregivermobile ?? ''],
    ['Caregiver Home Phone', form.caregiverhome ?? ''],
    ['Caregiver Work Phone', form.caregiverwork ?? ''],
    ['Photos on social media', yesNo(form.photossocialmedia)],
    ['Anyone else pick up / drop off', yesNo(form.cananyoneelsepickup)],
    ['Who can pick up', form.whocanpickup ?? ''],
    ['Who CANNOT pick up', form.whocannotpickup ?? ''],
  ];
  return XLSX.utils.aoa_to_sheet(rows);
}

// Build a single-row workbook for the Medical tab
function buildMedicalSheet(form) {
  const rows = [
    ['Field', 'Value'],
    ['Any allergies?', yesNo(form.allergies)],
    ['Allergy details / emergency medication', form.allergydetails ?? ''],
    ['Medical or physical conditions?', yesNo(form.medicalyn)],
    ['Medications or aids', form.medicaldetails ?? ''],
  ];
  return XLSX.utils.aoa_to_sheet(rows);
}

// Build a multi-row workbook for the Training tab
function buildTrainingSheet(training) {
  const header = ['Date', 'Training', 'Unit Leader', 'Hours'];
  const data = training.map((t) => [
    formatDate(t.thedate),
    t.Training ?? '',
    t.UnitLeader ?? '',
    t.hours ?? '',
  ]);
  return XLSX.utils.aoa_to_sheet([header, ...data]);
}

// Helper: write a sheet into a new workbook and return the binary output
function sheetToBlob(sheet) {
  const wb = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(wb, sheet, 'Sheet1');
  return XLSX.write(wb, { bookType: 'xlsx', type: 'array' });
}

// Download a document file as an ArrayBuffer via the API
async function fetchDocumentFile(api, docId) {
  const url = ApiFileUrl(`/cadets/documents/${docId}/file`);
  const resp = await api.get(url, { responseType: 'arraybuffer' });
  return resp.data;
}

// Sanitise a string for use as a filename
function safeName(name) {
  return (name || 'Cadet').replace(/[^\w\s-]/g, '').trim();
}

/**
 * Export a single cadet:
 *  - Details.xlsx, Medical.xlsx, Training.xlsx
 *  - Every uploaded document
 *  All bundled into "<Cadet Name>.zip" and downloaded.
 */
export async function exportCadet(api, form, training, documents, crews) {
  const zip = new JSZip();
  const cadetName = safeName(`${form.firstname} ${form.lastname}`.trim());

  // Excel sheets
  zip.file('Details.xlsx', sheetToBlob(buildDetailsSheet(form, crews)));
  zip.file('Medical.xlsx', sheetToBlob(buildMedicalSheet(form)));
  zip.file('Training.xlsx', sheetToBlob(buildTrainingSheet(training)));

  // Documents – download each file and add to the zip
  const docFolder = zip.folder('Documents');
  await Promise.all(
    documents.map(async (d) => {
      try {
        const data = await fetchDocumentFile(api, d.id);
        const filename = d.origfilename || `document_${d.id}`;
        docFolder.file(filename, data);
      } catch {
        // Skip documents that fail to download
      }
    })
  );

  // Generate the zip and trigger download
  const blob = await zip.generateAsync({ type: 'blob' });
  saveAs(blob, `${cadetName}.zip`);
}
