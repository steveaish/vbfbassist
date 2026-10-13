import React from 'react'
import ReactDOM from 'react-dom/client'
import '@fontsource/roboto/400.css'
import '@fontsource/roboto/500.css'
import App from './App.jsx'
import './index.css'

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <App>
      <div className="vbfb-footer">
        Site Version {process.env.REACT_APP_CURRENTVERSION} - by <a href="https://tonictechnologies.com.au" target="_blank" rel="noreferrer">Tonic Technologies</a>
      </div>
    </App>
  </React.StrictMode>
)
