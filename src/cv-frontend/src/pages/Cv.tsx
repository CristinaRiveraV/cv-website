import { useState, useEffect } from 'react'
import {
  Typography, Card, CardContent, Box, Grid, LinearProgress,
  List, ListItem, ListItemText, CircularProgress, Alert
} from '@mui/material'
import HomeIcon from '@mui/icons-material/Home'
import type { Person, Skill } from '../types/cv'

function Cv() {
  const [cv, setCv] = useState<Person | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetch(`${import.meta.env.VITE_API_URL}/cv`)
      .then(response => {
        if (!response.ok) throw new Error(`API error: ${response.status}`)
        return response.json()
      })
      .then(data => setCv(data))
      .catch(err => setError(err.message))
  }, [])

  if (error) return <Alert severity="error">{error}</Alert>
  if (!cv) return <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}><CircularProgress /></Box>

  return (
    <Box>
      <Grid container spacing={4}>
        <Grid size={{ xs: 12, md: 4 }}>
          <Box>
            <Typography variant="h6" color="primary" sx={{ mt: 0.5, mb: 2 }}>
              {cv.identity.jobTitle}
            </Typography>
          </Box>

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mb: 2 }}>
            <HomeIcon fontSize="small" color="primary" />
            <Typography variant="body2" color="text.secondary">
              {cv.identity.location}
            </Typography>
          </Box>

          <Typography variant="h4" component="h2" sx={{ borderBottom: 2, borderColor: 'primary.main', pb: 1, mb: 2 }}>
            Skills
          </Typography>
          {Object.entries(
            cv.allSkills.reduce<Record<string, Skill[]>>((acc, s) => {
              (acc[s.category] ??= []).push(s)
              return acc
            }, {})
          ).map(([category, skills]) => (
            <Box key={category} sx={{ mb: 2 }}>
              <Typography variant="subtitle2" sx={{ fontWeight: 'bold', mb: 1 }}>
                {category}
              </Typography>
              {skills.map((skill, i) => (
                <Box key={i} sx={{ mb: 1 }}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Typography variant="body2">{skill.name}</Typography>
                  </Box>
                  <LinearProgress variant="determinate" value={skill.proficiency * 10} />
                </Box>
              ))}
            </Box>
          ))}

          <Box sx={{ mb: 4 }}>
            <Typography variant="h4" component="h2" sx={{ borderBottom: 2, borderColor: 'primary.main', pb: 1, mb: 2 }}>
              Languages
            </Typography>
            <List dense>
              {cv.languages.map((lang, i) => (
                <ListItem key={i}>
                  <ListItemText primary={lang.name} secondary={lang.proficiency} />
                </ListItem>
              ))}
            </List>
          </Box>
        </Grid>

        <Grid size={{ xs: 12, md: 8 }}>
          <Box sx={{ mb: 4 }}>
            <Typography>{cv.identity.personalSummary}</Typography>
          </Box>

          <Box sx={{ mb: 4 }}>
            <Typography variant="h4" component="h2" sx={{ borderBottom: 2, borderColor: 'primary.main', pb: 1, mb: 2 }}>
              Experience
            </Typography>
            {cv.experiences.map(exp => (
              <Card key={exp.id} sx={{ mb: 2 }} variant="outlined">
                <CardContent>
                  <Typography variant="h6">{exp.role}</Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                    {exp.company} — {exp.location} ({exp.mode})
                  </Typography>
                  <Typography sx={{ mb: 1 }}>{exp.summary}</Typography>
                  <List dense disablePadding>
                    {exp.responsibilities.map((r, i) => (
                      <ListItem key={i} sx={{ pl: 2 }}>
                        <ListItemText primary={r.description} />
                      </ListItem>
                    ))}
                  </List>
                </CardContent>
              </Card>
            ))}
          </Box>

          <Box sx={{ mb: 4 }}>
            <Typography variant="h4" component="h2" sx={{ borderBottom: 2, borderColor: 'primary.main', pb: 1, mb: 2 }}>
              Education
            </Typography>
            {cv.education.map(edu => (
              <Card key={edu.id} sx={{ mb: 2 }} variant="outlined">
                <CardContent>
                  <Typography variant="h6">{edu.name}</Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                    {edu.institution} — {edu.location} ({edu.endYear})
                  </Typography>
                  <Typography>{edu.description}</Typography>
                </CardContent>
              </Card>
            ))}
          </Box>
        </Grid>
      </Grid>
    </Box>
  )
}

export default Cv