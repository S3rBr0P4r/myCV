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

export interface Education {
  degree: string;
  institution: string;
  notes: string;
}

export interface Certification {
  category: string;
  title: string;
  issuer: string;
}

export interface CV {
  name: string;
  lastName: string;
  title: string;
  summary: string;
  contactInfo?: ContactInfo | null;
  experiences: Experience[];
  skillCategories: SkillCategory[];
  education: Education[];
  certifications: Certification[];
}
