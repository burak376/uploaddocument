import React, { createContext, useContext, useState } from 'react';

interface Company {
  id: string;
  name: string;
  taxNumber: string;
  address: string;
  phone: string;
  email: string;
  isActive: boolean;
}

interface DocumentType {
  id: string;
  name: string;
  description: string;
  allowedExtensions: string[];
  maxFileSize: number; // MB cinsinden
  companyId?: string;
}

interface Document {
  id: string;
  name: string;
  originalName: string;
  fileSize: number;
  fileExtension: string;
  documentTypeId: string;
  documentTypeName: string;
  uploadedById: string;
  uploadedByName: string;
  uploadDate: string;
  companyId: string;
  filePath: string;
}

interface AppContextType {
  companies: Company[];
  documentTypes: DocumentType[];
  documents: Document[];
  addCompany: (company: Omit<Company, 'id'>) => void;
  updateCompany: (id: string, company: Partial<Company>) => void;
  deleteCompany: (id: string) => void;
  addDocumentType: (documentType: Omit<DocumentType, 'id'>) => void;
  updateDocumentType: (id: string, documentType: Partial<DocumentType>) => void;
  deleteDocumentType: (id: string) => void;
  addDocument: (document: Omit<Document, 'id'>) => void;
  deleteDocument: (id: string) => void;
}

const AppContext = createContext<AppContextType | undefined>(undefined);

// Mock veriler
const mockCompanies: Company[] = [
  {
    id: '1',
    name: 'Bugibo Yazılım',
    taxNumber: '1234567890',
    address: 'İstanbul, Türkiye',
    phone: '0212 123 45 67',
    email: 'info@bugibo.com',
    isActive: true
  },
  {
    id: '2',
    name: 'TechCorp Ltd.',
    taxNumber: '0987654321',
    address: 'Ankara, Türkiye',
    phone: '0312 987 65 43',
    email: 'info@techcorp.com',
    isActive: true
  }
];

const mockDocumentTypes: DocumentType[] = [
  {
    id: '1',
    name: 'TC Kimlik',
    description: 'TC Kimlik belgesi',
    allowedExtensions: ['.pdf', '.jpg', '.png'],
    maxFileSize: 5,
    companyId: '1'
  },
  {
    id: '2',
    name: 'Özlük Hakları',
    description: 'Özlük hakları belgeleri',
    allowedExtensions: ['.pdf', '.docx'],
    maxFileSize: 10,
    companyId: '1'
  },
  {
    id: '3',
    name: 'E-Defter',
    description: 'E-Defter excel dosyaları',
    allowedExtensions: ['.xlsx', '.xls'],
    maxFileSize: 20,
    companyId: '1'
  }
];

const mockDocuments: Document[] = [
  {
    id: '1',
    name: 'burak_tc_kimlik_2024.pdf',
    originalName: 'tc_kimlik.pdf',
    fileSize: 1.2,
    fileExtension: '.pdf',
    documentTypeId: '1',
    documentTypeName: 'TC Kimlik',
    uploadedById: '3',
    uploadedByName: 'Burak Kullanıcı',
    uploadDate: '2024-01-15T10:30:00',
    companyId: '1',
    filePath: '/uploads/1/burak/tc-kimlik/2024/01/burak_tc_kimlik_2024.pdf'
  },
  {
    id: '2',
    name: 'ozluk_haklari_2024.docx',
    originalName: 'özlük_hakları.docx',
    fileSize: 0.8,
    fileExtension: '.docx',
    documentTypeId: '2',
    documentTypeName: 'Özlük Hakları',
    uploadedById: '3',
    uploadedByName: 'Burak Kullanıcı',
    uploadDate: '2024-01-20T14:15:00',
    companyId: '1',
    filePath: '/uploads/1/burak/ozluk-haklari/2024/01/ozluk_haklari_2024.docx'
  }
];

export const AppProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [companies, setCompanies] = useState<Company[]>(mockCompanies);
  const [documentTypes, setDocumentTypes] = useState<DocumentType[]>(mockDocumentTypes);
  const [documents, setDocuments] = useState<Document[]>(mockDocuments);

  const addCompany = (company: Omit<Company, 'id'>) => {
    const newCompany: Company = {
      ...company,
      id: Date.now().toString()
    };
    setCompanies(prev => [...prev, newCompany]);
  };

  const updateCompany = (id: string, company: Partial<Company>) => {
    setCompanies(prev => prev.map(c => c.id === id ? { ...c, ...company } : c));
  };

  const deleteCompany = (id: string) => {
    setCompanies(prev => prev.filter(c => c.id !== id));
  };

  const addDocumentType = (documentType: Omit<DocumentType, 'id'>) => {
    const newDocumentType: DocumentType = {
      ...documentType,
      id: Date.now().toString()
    };
    setDocumentTypes(prev => [...prev, newDocumentType]);
  };

  const updateDocumentType = (id: string, documentType: Partial<DocumentType>) => {
    setDocumentTypes(prev => prev.map(dt => dt.id === id ? { ...dt, ...documentType } : dt));
  };

  const deleteDocumentType = (id: string) => {
    setDocumentTypes(prev => prev.filter(dt => dt.id !== id));
  };

  const addDocument = (document: Omit<Document, 'id'>) => {
    const newDocument: Document = {
      ...document,
      id: Date.now().toString()
    };
    setDocuments(prev => [...prev, newDocument]);
  };

  const deleteDocument = (id: string) => {
    setDocuments(prev => prev.filter(d => d.id !== id));
  };

  return (
    <AppContext.Provider value={{
      companies,
      documentTypes,
      documents,
      addCompany,
      updateCompany,
      deleteCompany,
      addDocumentType,
      updateDocumentType,
      deleteDocumentType,
      addDocument,
      deleteDocument
    }}>
      {children}
    </AppContext.Provider>
  );
};

export const useApp = () => {
  const context = useContext(AppContext);
  if (context === undefined) {
    throw new Error('useApp must be used within an AppProvider');
  }
  return context;
};