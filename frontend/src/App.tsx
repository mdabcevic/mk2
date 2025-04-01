
import './App.css'
import AppRoutes from './AppRoutes'

function App() {

  
  let token = localStorage.getItem("token");
  if(!token)//remove it later!!!
    localStorage.setItem("token","eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIxIiwicm9sZSI6Im1hbmFnZXIiLCJ1c2VybmFtZSI6InZpdmFzX2lsaWNhIiwiZXhwIjo5OTk5OTk5OTk5fQ.0XHGglQEhRivWpRzlZjKUwGoRCCbnBJ0ARTLMr3jv_o")

  return (
    <>
      <AppRoutes />
    </>
  )
}

export default App
