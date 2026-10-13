// Permission flags come from the API at login (users + matching profiles row) and live in sessionStorage.
// isadmin implies every other profile permission (the API already folds that in).

export const getPermissions = () => {
    try {
        return JSON.parse(sessionStorage.getItem('permissions') ?? '{}');
    }
    catch {
        return {};
    }
};

// name can be one flag or a list (any of them)
export const hasPermission = (name) => {
    const permissions = getPermissions();
    return (Array.isArray(name) ? name : [name]).some((n) => permissions[n] === true);
};

export const isCurrentUserAdmin = () => hasPermission('isadmin');

export const isSiteAdmin = () => hasPermission('siteadmin');
