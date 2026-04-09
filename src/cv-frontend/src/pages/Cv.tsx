import { useState, useEffect } from 'react'
import type { Person } from '../types/cv'

function Cv() {
  const [cv, setCv] = useState<Person | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetch('http://localhost:5123/cv')
      .then(response => {
        if (!response.ok) throw new Error(`API error: ${response.status}`)
        return response.json()
      })
      .then(data => setCv(data))
      .catch(err => setError(err.message))
  }, [])

  if (error) return <p>Error: {error}</p>
  if (!cv) return <p>Loading...</p>

  return (
    <div>
      <header>
        <h1>{cv.identity.name}</h1>
        <p className="job-title">{cv.identity.jobTitle}</p>
        <p>{cv.identity.personalSummary}</p>
      </header>

      <section>
        <h2>Experience</h2>
        {cv.experiences.map(exp => (
          <div key={exp.id} className="card">
            <h3>{exp.role}</h3>
            <p className="subtitle">
              {exp.company} — {exp.location} ({exp.mode})
            </p>
            <p>{exp.summary}</p>
            <ul>
              {exp.responsibilities.map((r, i) => (
                <li key={i}>{r.description}</li>
              ))}
            </ul>
          </div>
        ))}
      </section>

      <section>
        <h2>Education</h2>
        {cv.education.map(edu => (
          <div key={edu.id} className="card">
            <h3>{edu.name}</h3>
            <p className="subtitle">
              {edu.institution} — {edu.location} ({edu.endYear})
            </p>
            <p>{edu.description}</p>
          </div>
        ))}
      </section>

      <section>
        <h2>Skills</h2>
        <div className="skills-list">
          {cv.allSkills.map((skill, i) => (
            <span key={i} className="skill-tag">
              {skill.name}
            </span>
          ))}
        </div>
      </section>

      <section>
        <h2>Languages</h2>
        <ul>
          {cv.languages.map((lang, i) => (
            <li key={i}>{lang.name} — {lang.proficiency}</li>
          ))}
        </ul>
      </section>
    </div>
  )
}

export default Cv