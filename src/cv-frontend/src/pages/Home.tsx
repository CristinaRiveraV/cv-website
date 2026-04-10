import { Typography, Box, Button } from '@mui/material'
import { Link } from 'react-router-dom'

function Home() {
  return (
    <Box sx={{ textAlign: 'center', py: 8 }}>
      <Typography variant="h3" component="h1" gutterBottom>
        Welcome!
      </Typography>
      <Typography variant="h6" color="text.secondary" sx={{ mb: 4 }}>
        Thanks for stopping by. Have a look around.
      </Typography>
      <Button variant="contained" component={Link} to="/cv" size="large">
        View my CV
      </Button>
    </Box>
  )
}

export default Home