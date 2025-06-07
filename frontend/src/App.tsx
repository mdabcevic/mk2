
import { BrowserRouter } from 'react-router-dom'
import './styles/App.css'
import AppRoutes from './utils/routing/AppRoutes'
import ScrollToTop from './utils/scroll-to-top'

function App() {

  return (
    <> 
      <BrowserRouter>
        <ScrollToTop />
        <AppRoutes />
      </BrowserRouter>
      
    </>
  )
}

export default App
