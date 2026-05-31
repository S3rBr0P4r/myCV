export interface Experience {
  period: string;
  role: string;
  company: string;
  description: string;
  background?: string;
}

export interface CV {
  name: string;
  lastName: string;
  title: string;
  summary: string;
  experiences: Experience[];
  skills: string[];
}
