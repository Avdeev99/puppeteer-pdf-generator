import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '30s', target: 15 },  // Ramp up to 5 users over 30 seconds
    // { duration: '1m', target: 5 },   // Stay at 5 users for 1 minute
    // { duration: '30s', target: 0 },  // Ramp down to 0 users
  ],
  thresholds: {
    http_req_failed: ['rate<0.01'],    // Less than 1% of requests should fail
  },
};

const BASE_URL = 'http://localhost:3000';

export default function () {
  const payload = {
    jsonrpc: '2.0',
    method: 'generatePdf',
    params: {
      contentHtml: '<html><body><h1>Test PDF Generation</h1><p>Generated at: ' + new Date().toISOString() + '</p></body></html>',
      pdfOptions: {
        format: 'A4',
        printBackground: true,
        displayHeaderFooter: true,
        margin: {
          top: '1cm',
          bottom: '1cm'
        },
        headerTemplate: '<div style="font-size: 10px; text-align: center; width: 100%;">Header</div>',
        footerTemplate: '<div style="font-size: 10px; text-align: center; width: 100%;">Footer</div>'
      }
    },
    id: Date.now().toString()
  };

  const headers = {
    'Content-Type': 'application/json',
  };

  const response = http.post(
    `${BASE_URL}/api/rpc`,
    JSON.stringify(payload),
    { headers }
  );

  // Verify the response
  check(response, {
    'is status 200': (r) => r.status === 200,
    'has valid JSON-RPC response': (r) => {
      const body = JSON.parse(r.body);
      return body.result || (body.error && body.error.code);
    },
  });

  sleep(1); // Wait 1 second between requests
}