import { useEffect, useState } from 'react';
import { DataManager, UrlAdaptor } from '@syncfusion/ej2-data';
import { ApiHeaders, GetApiUrl } from './ApiUtils';

// Server-side Syncfusion grid source: POSTs paging/sort/filter to <api>/<endpoint> which returns { result, count }
export function createGridDataManager(endpoint) {
    return new DataManager({
        url: GetApiUrl() + endpoint,
        adaptor: new UrlAdaptor(),
        headers: [ApiHeaders()]
    });
}

// Columns marked with hideAtMedia={DESKTOP_ONLY} disappear on phones
export const DESKTOP_ONLY = '(min-width: 768px)';

export function useIsMobile() {
    const query = '(max-width: 767.98px)';
    const [isMobile, setIsMobile] = useState(() => window.matchMedia(query).matches);

    useEffect(() => {
        const mq = window.matchMedia(query);
        const onChange = (e) => setIsMobile(e.matches);
        mq.addEventListener('change', onChange);
        return () => mq.removeEventListener('change', onChange);
    }, []);

    return isMobile;
}

// dd/mm/yyyy for display; accepts ISO strings or Date
export function formatDate(value) {
    if (!value) return '';
    const d = value instanceof Date ? value : new Date(value);
    if (isNaN(d)) return '';
    return d.toLocaleDateString('en-AU', { day: '2-digit', month: '2-digit', year: 'numeric' });
}

export const yesNo = (v) => (v === 'Y' ? 'Yes' : v === 'N' ? 'No' : '');
