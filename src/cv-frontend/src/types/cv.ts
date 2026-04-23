export interface Identity {
    name: string;
    jobTitle: string;
    personalSummary: string;
    location: string;
}

export interface ContactInformation {
    email: string;
    phone: string | null;
    linkedIn: string;
    gitHub: string;
    portfolio: string | null;
}

export interface Experience {
    id: string;
    role: string;
    company: string;
    location: string;
    summary: string;
    startDate: string;
    endDate: string | null;
    mode: string;
    skills: Skill[];
    responsibilities: Responsibility[];
}

export interface Responsibility {
    description: string;
}

export interface Education {
    id: string;
    name: string;
    institution: string;
    location: string;
    startYear: number | null;
    endYear: number;
    description: string;
}

export interface Skill {
    name: string;
    category: string;
    proficiency: number;
}

export interface Language {
    name: string;
    proficiency: string;
}

export interface Person {
    identity: Identity;
    experiences: Experience[];
    education: Education[];
    projects: Project[];
    additionalSkills: Skill[];
    languages: Language[];
    allSkills: Skill[];
}

export interface Project {
    id: string;
    name: string;
    description: string;
    skills: Skill[];
}