import { BrowserRouter, Routes, Route, NavLink } from 'react-router-dom'
import { AppBar, Toolbar, Button, Box, Typography } from '@mui/material'
import Home from './pages/Home'
import Cv from './pages/Cv'
import Contact from './pages/Contact'

function App() {
  return (
    <BrowserRouter>
      <AppBar position="static">
        <Toolbar>
          <Typography variant="h6" sx={{ flexGrow: 1 }}>
            Cristina Rivera Valdez
          </Typography>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Button color="inherit" component={NavLink} to="/">Home</Button>
            <Button color="inherit" component={NavLink} to="/cv">CV</Button>
            <Button color="inherit" component={NavLink} to="/contact">Contact</Button>
          </Box>
        </Toolbar>
      </AppBar>

      <Box component="main" sx={{ maxWidth: 900, mx: 'auto', py: 4, px: 3 }}>
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/cv" element={<Cv />} />
          <Route path="/contact" element={<Contact />} />
        </Routes>
      </Box>
    </BrowserRouter>
  )
}

export default App