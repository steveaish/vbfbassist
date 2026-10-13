// VEH_Check_Header.status values (see VehicleChecksController)
export const checkStatus = {
  0: { label: 'New', color: 'default' },
  1: { label: 'In Progress', color: 'warning' },
  2: { label: 'Check Complete', color: 'info' },
  3: { label: 'Ready To Sign Off', color: 'primary' },
  4: { label: 'Signed Off', color: 'success' }
};

export const SIGNED_OFF = 4;

export const results = {
  notChecked: 'Not Checked',
  pass: 'Checked Pass',
  fail: 'Checked Fail'
};
