// Main menu, mirroring the Laravel layout (resources/views/layouts/main.blade.php).
// permission = a flag (or list of flags, any of them) for LoginUtils.hasPermission(); none = everyone logged in.
export const menuItems = [
  { key: 'mypage', label: 'My Page', path: '/mypage' },
  { key: 'members', label: 'Members', path: '/members', permission: 'viewmembers' },
  { key: 'profiles', label: 'Profiles', path: '/profiles', permission: 'isadmin' },
  { key: 'cadets', label: 'Cadets', path: '/cadets', permission: 'cadetleader' },
  { key: 'bfr', label: 'Bush Fire Ready', path: '/bfr', permission: ['isfacilitator', 'iscoordinator'] },
  { key: 'vehicles', label: 'Vehicle Maint', path: '/vehicles', permission: 'isvehiclemaint' },
  { key: 'ppe', label: 'PPE Items', path: '/ppe', permission: 'isadmin' },
  { key: 'calendar', label: 'Calendar', path: '/calendar' },
  { key: 'documents', label: 'Documents', path: '/documents' },
  { key: 'bulksms', label: 'Bulk SMS', path: '/bulksms', permission: 'canbulksms' }
];
