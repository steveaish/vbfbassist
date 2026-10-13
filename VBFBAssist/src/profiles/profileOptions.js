// Membership / role flags (Y/N columns on profiles) - also the "type" filter on the Profiles list
export const membershipFlags = [
  { field: 'firstlinefirefighter', label: '1st Line Firefighter' },
  { field: 'secondlinefirefighter', label: '2nd Line Firefighter' },
  { field: 'president', label: 'President' },
  { field: 'captain', label: 'Captain' },
  { field: 'lieutenant', label: 'Lieutenant' },
  { field: 'otherofficer', label: 'Other Officer' },
  { field: 'cadet', label: 'Cadet' },
  { field: 'BFRResident', label: 'Bush Fire Ready Resident' },
  { field: 'lifemember', label: 'Life Member' },
  { field: 'auxmember', label: 'Auxiliary Member' }
];

export const accessFlags = [
  { field: 'isadmin', label: 'Admin' },
  { field: 'ViewMembers', label: 'View Members' },
  { field: 'IsVehicleMaint', label: 'Vehicle Maintenance' },
  { field: 'cadetleader', label: 'Cadet Leader' },
  { field: 'canbulksms', label: 'Can Bulk SMS' },
  { field: 'caneditcalendar', label: 'Can Edit Calendar' },
  { field: 'caneditdocuments', label: 'Can Edit Documents' }
];

export const bfrFlags = [
  { field: 'isfacilitator', label: 'Bush Fire Ready Facilitator' },
  { field: 'iscoordinator', label: 'Bush Fire Ready Street Coordinator' }
];

export const medalFlags = [
  { field: 'year5', label: '5 Year Medal' },
  { field: 'year10', label: '10 Year Medal' },
  { field: 'year15', label: '15 Year Medal' },
  { field: 'year20', label: '20 Year Medal' },
  { field: 'year25', label: '25 Year Medal' },
  { field: 'year30', label: '30 Year Medal' }
];

// Options for the list's type filter (columns the API accepts)
export const typeFilterOptions = [
  { value: 'isadmin', label: 'Admin' },
  { value: 'isfacilitator', label: 'BFR Facilitator' },
  { value: 'iscoordinator', label: 'BFR Street Coordinator' },
  { value: 'firstlinefirefighter', label: '1st Line Firefighter' },
  { value: 'secondlinefirefighter', label: '2nd Line Firefighter' },
  { value: 'president', label: 'President' },
  { value: 'captain', label: 'Captain' },
  { value: 'lieutenant', label: 'Lieutenant' },
  { value: 'otherofficer', label: 'Other Officer' },
  { value: 'cadet', label: 'Cadet' },
  { value: 'cadetleader', label: 'Cadet Leader' },
  { value: 'lifemember', label: 'Life Member' },
  { value: 'auxmember', label: 'Auxiliary Member' }
];

export const sexOptions = [
  { value: 'Not Specified', label: 'Not Specified' },
  { value: 'Female', label: 'Female' },
  { value: 'Male', label: 'Male' }
];

export const licenceOptions = [
  { value: 'None', label: 'None' },
  { value: 'Car', label: 'Car' },
  { value: 'MR / HR', label: 'MR / HR' }
];

export const prefCommOptions = [
  { value: '', label: 'Not set' },
  { value: 'BART', label: 'BART' },
  { value: 'Email', label: 'Email' },
  { value: 'SMS', label: 'Text Message' }
];
