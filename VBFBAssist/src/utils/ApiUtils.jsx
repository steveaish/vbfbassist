import axios from "axios";

function ApiHeaders() {
    return {
        'APIKEY': process.env.REACT_APP_API_KEY,
        "UserId": sessionStorage.getItem('userid'),
        "UserKey": sessionStorage.getItem('userkey'),
        Accept: 'application/json',
        'Content-Type': 'application/json'
    }
}

// For multipart uploads - the browser sets Content-Type with the boundary
function ApiFileHeaders() {
    return {
        'APIKEY': process.env.REACT_APP_API_KEY,
        "UserId": sessionStorage.getItem('userid'),
        "UserKey": sessionStorage.getItem('userkey')
    }
}

// Links opened in a new tab (PDFs, downloads) can't send headers, so they carry them in the query
function ApiQueryAuth() {
    const params = new URLSearchParams({
        apikey: process.env.REACT_APP_API_KEY,
        userid: sessionStorage.getItem('userid') ?? '',
        userkey: sessionStorage.getItem('userkey') ?? ''
    });
    return params.toString();
}

// Full URL for a file/PDF endpoint opened in a new tab (auth goes in the query string)
function ApiFileUrl(path, extra = "") {
    return `${GetApiUrl()}${path}?${ApiQueryAuth()}${extra}`;
}

function GetApiUrl() {
    // Deployed origins get added here once hosting is decided
    if (process.env.NODE_ENV == 'development') {
        return process.env.REACT_APP_API_URL_DEV;
    }
    return process.env.REACT_APP_API_URL_PROD;
}

// axios instance used by every screen: adds the auth headers and sends the user back to the
// login page when the session key has expired
const api = axios.create();

api.interceptors.request.use((config) => {
    config.baseURL = GetApiUrl();
    config.headers = { ...ApiHeaders(), ...config.headers };
    // Let the browser set multipart/form-data with its boundary for uploads
    if (config.data instanceof FormData) delete config.headers["Content-Type"];
    return config;
});

api.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401 && sessionStorage.getItem('loggedIn') === 'true') {
            ClearSession();
            window.location.replace('/');
        }
        return Promise.reject(error);
    }
);

function SaveSession(data) {
    sessionStorage.setItem('loggedIn', 'true');
    sessionStorage.setItem('userid', data.userid);
    sessionStorage.setItem('userkey', data.userkey);
    sessionStorage.setItem('username', data.name ?? '');
    sessionStorage.setItem('email', data.email ?? '');
    sessionStorage.setItem('firstname', data.firstname ?? '');
    sessionStorage.setItem('profileid', data.profileid ?? '');
    sessionStorage.setItem('permissions', JSON.stringify(data.permissions ?? {}));
}

function ClearSession() {
    sessionStorage.clear();
}

function ApiErrorMessage(error, fallback = 'Something went wrong. Please try again.') {
    return error?.response?.data?.message ?? fallback;
}

export { api, ApiHeaders, ApiFileHeaders, ApiQueryAuth, ApiFileUrl, GetApiUrl, SaveSession, ClearSession, ApiErrorMessage };
