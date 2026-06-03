export interface Experience {
  period: string;
  role: string;
  company: string;
  companyUrl?: string;
  location?: string;
  workMode?: string;
  description: string;
  background?: string;
}

export interface ContactInfo {
  email: string;
  phone: string;
  location: string;
  willingnessToTravel: string;
}

export interface SkillSubCategory {
  name: string;
  items: string[];
}

export interface SkillCategory {
  name: string;
  subCategories: SkillSubCategory[];
}

export interface CV {
  name: string;
  lastName: string;
  title: string;
  summary: string;
  contactInfo?: ContactInfo | null;
  experiences: Experience[];
  skillCategories: SkillCategory[];
  linkedInUrl?: string;
  gitHubUrl?: string;
}
