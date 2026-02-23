const target = `http://127.0.0.1:5069`

const PROXY_CONFIG = [
  {
    context: [
      "/EmployeeCollaboration/**",
   ],
    proxyTimeout: 10000,
    target: target,
    secure: false,
    changeOrigin: true,
    headers: {
      Connection: 'Keep-Alive',
    }
  }
]

module.exports = PROXY_CONFIG;
