import React, { useState } from 'react';
import { useEffect } from 'react';
import { FileText, Download, Trash2, Eye, Calendar, User } from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';
import { useApp } from '../contexts/AppContext';
import { documentService } from '../services/documentService';
import toast from 'react-hot-toast';

const MyDocuments: React.FC = () => {
  const { user } = useAuth();
  const { documents, setDocuments, deleteDocument } = useApp();
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedDocumentType, setSelectedDocumentType] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadDocuments();
  }, [user?.id]);

  const loadDocuments = async () => {
    if (!user) return;
    
    try {
      setLoading(true);
      const data = await documentService.getAll();
      setDocuments(data);
    } catch (error) {
      toast.error('Belgeler yüklenirken hata oluştu');
    } finally {
      setLoading(false);
    }
  };

  // Kullanıcının belgelerini filtrele
  const myDocuments = documents.filter(doc => {
    const isMyDocument = user?.role === 'User' 
      ? doc.uploadedById === user.id
      : doc.companyId === user?.companyId;
    
    if (!isMyDocument) return false;

    const matchesSearch = doc.originalName.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         doc.documentTypeName.toLowerCase().includes(searchTerm.toLowerCase());
    
    const matchesType = selectedDocumentType === '' || doc.documentTypeId === parseInt(selectedDocumentType);
    
    return matchesSearch && matchesType;
  });

  // Belge türlerini al
  const documentTypes = Array.from(new Set(myDocuments.map(doc => ({
    id: doc.documentTypeId,
    name: doc.documentTypeName
  }))));

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('tr-TR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const formatFileSize = (size: number) => {
    return `${size} MB`;
  };

  const handleDownload = async (document: any) => {
    try {
      setLoading(true);
      const blob = await documentService.download(document.id);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = document.originalName;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
      toast.success(`${document.originalName} indirildi`);
    } catch (error) {
      toast.error('Dosya indirilemedi');
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (document: any) => {
    if (window.confirm(`${document.originalName} dosyasını silmek istediğinizden emin misiniz?`)) {
      try {
        await documentService.delete(document.id);
        deleteDocument(document.id);
        toast.success('Belge başarıyla silindi');
      } catch (error: any) {
        toast.error(error.response?.data?.message || 'Silme işlemi başarısız');
      }
    }
  };

  const handleView = (document: any) => {
    handleDownload(document);
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">
          {user?.role === 'User' ? 'Belgelerim' : 'Firma Belgeleri'}
        </h1>
        <p className="text-gray-600">
          {user?.role === 'User' 
            ? 'Yüklediğiniz belgeler' 
            : 'Firmadaki tüm belgeler'
          }
        </p>
      </div>

      {/* Filtreleme */}
      <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label htmlFor="search" className="block text-sm font-medium text-gray-700 mb-2">
              Belge Ara
            </label>
            <input
              id="search"
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder="Belge adı veya türü ara..."
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
          <div>
            <label htmlFor="documentType" className="block text-sm font-medium text-gray-700 mb-2">
              Belge Türü
            </label>
            <select
              id="documentType"
              value={selectedDocumentType}
              onChange={(e) => setSelectedDocumentType(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="">Tüm türler</option>
              {documentTypes.map(type => (
                <option key={type.id} value={type.id}>
                  {type.name}
                </option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {/* Belgeler Listesi */}
      <div className="bg-white rounded-lg shadow-sm border border-gray-200">
        {myDocuments.length > 0 ? (
          <div>
            <div className="p-6 border-b border-gray-200">
              <h2 className="text-lg font-semibold text-gray-900">
                Belgeler ({myDocuments.length})
              </h2>
            </div>
            <div className="divide-y divide-gray-200">
              {myDocuments.map((document) => (
                <div key={document.id} className="p-6 hover:bg-gray-50 transition-colors">
                  <div className="flex items-center justify-between">
                    <div className="flex items-start space-x-4">
                      <div className="p-2 bg-blue-100 rounded-lg">
                        <FileText className="h-6 w-6 text-blue-600" />
                      </div>
                      <div className="flex-1">
                        <h3 className="text-lg font-medium text-gray-900 mb-1">
                          {document.originalName}
                        </h3>
                        <div className="flex items-center space-x-4 text-sm text-gray-600">
                          <span className="flex items-center">
                            <FileText className="h-4 w-4 mr-1" />
                            {document.documentTypeName}
                          </span>
                          <span className="flex items-center">
                            <Calendar className="h-4 w-4 mr-1" />
                            {formatDate(document.uploadDate)}
                          </span>
                          {user?.role !== 'User' && (
                            <span className="flex items-center">
                              <User className="h-4 w-4 mr-1" />
                              {document.uploadedByName}
                            </span>
                          )}
                        </div>
                        <div className="mt-2 flex items-center space-x-4 text-xs text-gray-500">
                          <span>Boyut: {formatFileSize(document.fileSize)}</span>
                          <span>Format: {document.fileExtension}</span>
                        </div>
                      </div>
                    </div>
                    
                    <div className="flex items-center space-x-2">
                      <button
                        onClick={() => handleView(document)}
                        className="p-2 text-blue-600 hover:bg-blue-100 rounded-lg transition-colors"
                        title="Görüntüle"
                      >
                        <Eye className="h-4 w-4" />
                      </button>
                      <button
                        onClick={() => handleDownload(document)}
                        className="p-2 text-green-600 hover:bg-green-100 rounded-lg transition-colors"
                        title="İndir"
                      >
                        <Download className="h-4 w-4" />
                      </button>
                      <button
                        onClick={() => handleDelete(document)}
                        className="p-2 text-red-600 hover:bg-red-100 rounded-lg transition-colors"
                        title="Sil"
                      >
                        <Trash2 className="h-4 w-4" />
                      </button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        ) : (
          <div className="text-center py-12">
            <FileText className="h-16 w-16 text-gray-400 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">
              Belge bulunamadı
            </h3>
            <p className="text-gray-600 mb-4">
              {searchTerm || selectedDocumentType 
                ? 'Arama kriterlerinize uygun belge bulunamadı.'
                : 'Henüz yüklenmiş belge bulunmuyor.'
              }
            </p>
            {!searchTerm && !selectedDocumentType && (
              <button className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors">
                İlk Belgenizi Yükleyin
              </button>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

export default MyDocuments;