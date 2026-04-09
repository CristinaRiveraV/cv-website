import { BrowserRouter, Routes, Route, NavLink } from 'react-router-dom'
import Home from './pages/Home'
import Cv from './pages/Cv'
import Contact from './pages/Contact'
import './App.css'

function App() {
  return (
    <BrowserRouter>
      <nav>
        <NavLink to="/">Home</NavLink>
        <NavLink to="/cv">CV</NavLink>
        <NavLink to="/contact">Contact</NavLink>
      </nav>

      <main>
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/cv" element={<Cv />} />
          <Route path="/contact" element={<Contact />} />
        </Routes>
      </main>
    </BrowserRouter>
  )
}

export default App