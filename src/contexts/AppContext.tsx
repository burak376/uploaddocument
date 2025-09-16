import React, { createContext, useContext, useState } from 'react';
import { Company } from '../services/companyService';
import { DocumentType } from '../services/documentTypeService';
import { Document } from '../services/documentService';

interface AppContextType {
  companies: Company[];
  documentTypes: DocumentType[];
  documents: Document[];
  setCompanies: (companies: Company[]) => void;
  setDocumentTypes: (documentTypes: DocumentType[]) => void;
  setDocuments: (documents: Document[]) => void;
  addCompany: (company: Company) => void;
  updateCompany: (id: number, company: Company) => void;
  deleteCompany: (id: number) => void;
  addDocumentType: (documentType: DocumentType) => void;
  updateDocumentType: (id: number, documentType: DocumentType) => void;
  deleteDocumentType: (id: number) => void;
  addDocument: (document: Document) => void;
  deleteDocument: (id: number) => void;
}

const AppContext = createContext<AppContextType | undefined>(undefined);

export const AppProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [companies, setCompanies] = useState<Company[]>([]);
  const [documentTypes, setDocumentTypes] = useState<DocumentType[]>([]);
  const [documents, setDocuments] = useState<Document[]>([]);

  const addCompany = (company: Company) => {
    setCompanies(prev => [...prev, company]);
  };

  const updateCompany = (id: number, updatedCompany: Company) => {
    setCompanies(prev => prev.map(c => c.id === id ? updatedCompany : c));
  };

  const deleteCompany = (id: number) => {
    setCompanies(prev => prev.filter(c => c.id !== id));
  };

  const addDocumentType = (documentType: DocumentType) => {
    setDocumentTypes(prev => [...prev, documentType]);
  };

  const updateDocumentType = (id: number, updatedDocumentType: DocumentType) => {
    setDocumentTypes(prev => prev.map(dt => dt.id === id ? updatedDocumentType : dt));
  };

  const deleteDocumentType = (id: number) => {
    setDocumentTypes(prev => prev.filter(dt => dt.id !== id));
  };

  const addDocument = (document: Document) => {
    setDocuments(prev => [...prev, document]);
  };

  const deleteDocument = (id: number) => {
    setDocuments(prev => prev.filter(d => d.id !== id));
  };

  return (
    <AppContext.Provider value={{
      companies,
      documentTypes,
      documents,
      setCompanies,
      setDocumentTypes,
      setDocuments,
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