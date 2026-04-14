import { useState, useEffect } from 'react'
import { Typography, Box, List, ListItem, ListItemIcon, ListItemText, CircularProgress, Alert, Link } from '@mui/material'
import EmailIcon from '@mui/icons-material/Email'
import LinkedInIcon from '@mui/icons-material/LinkedIn'
import GitHubIcon from '@mui/icons-material/GitHub'
import PhoneIcon from '@mui/icons-material/Phone'
import type { ContactInformation } from '../types/cv'

function Contact() {
  const [contact, setContact] = useState<ContactInformation | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetch(`${import.meta.env.VITE_API_URL}/cv/contact`)
      .then(response => {
        if (!response.ok) throw new Error(`API error: ${response.status}`)
        return response.json()
      })
      .then(data => setContact(data))
      .catch(err => setError(err.message))
  }, [])

  if (error) return <Alert severity="error">{error}</Alert>
  if (!contact) return <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}><CircularProgress /></Box>

  return (
    <Box>
      <Typography variant="h4" component="h1" gutterBottom>
        Get in Touch
      </Typography>
      <List>
        <ListItem>
          <ListItemIcon><EmailIcon /></ListItemIcon>
          <ListItemText primary={<Link href={`mailto:${contact.email}`}>{contact.email}</Link>} />
        </ListItem>
        <ListItem>
          <ListItemIcon><LinkedInIcon /></ListItemIcon>
          <ListItemText primary={<Link href={contact.linkedIn} target="_blank" rel="noopener">LinkedIn</Link>} />
        </ListItem>
        <ListItem>
          <ListItemIcon><GitHubIcon /></ListItemIcon>
          <ListItemText primary={<Link href={contact.gitHub} target="_blank" rel="noopener">GitHub</Link>} />
        </ListItem>
      </List>
      {contact.phone && (
        <ListItem>
          <ListItemIcon><PhoneIcon /></ListItemIcon>
          <ListItemText primary={<Link href={`tel:${contact.phone}`}>{contact.phone}</Link>} />
        </ListItem>
      )}
    </Box>
  )
}

export default Contact