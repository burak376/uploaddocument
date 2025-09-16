import React from 'react';
import { useEffect } from 'react';
import { 
  Users, 
  Building2, 
  FileText, 
  Upload,
  TrendingUp,
  Calendar,
  Clock,
  CheckCircle 
} from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';
import { useApp } from '../contexts/AppContext';
import { companyService } from '../services/companyService';
import { documentService } from '../services/documentService';
import { documentTypeService } from '../services/documentTypeService';

const Dashboard: React.FC = () => {
  const { user } = useAuth();
  const { companies, documents, documentTypes, setCompanies, setDocuments, setDocumentTypes } = useApp();

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      if (user?.role === 'SuperAdmin') {
        const companiesData = await companyService.getAll();
        setCompanies(companiesData);
      }
      
      const documentsData = await documentService.getAll();
      setDocuments(documentsData);
      
      const documentTypesData = await documentTypeService.getAll();
      setDocumentTypes(documentTypesData);
    } catch (error) {
      console.error('Error loading dashboard data:', error);
    }
  };

  const userCompanyDocuments = user?.companyId 
    ? documents.filter(doc => doc.companyId === user.companyId)
    : documents;

  const myDocuments = documents.filter(doc => doc.uploadedById === user?.id);

  const recentDocuments = userCompanyDocuments
    .sort((a, b) => new Date(b.uploadDate).getTime() - new Date(a.uploadDate).getTime())
    .slice(0, 5);

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('tr-TR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
        <p className="text-gray-600">
          Hoş geldiniz, {user?.firstName} {user?.lastName}
        </p>
      </div>

      {/* İstatistik Kartları */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {user?.role === 'SuperAdmin' && (
          <>
            <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
              <div className="flex items-center">
                <div className="p-2 bg-blue-100 rounded-lg">
                  <Building2 className="h-6 w-6 text-blue-600" />
                </div>
                <div className="ml-4">
                  <p className="text-sm font-medium text-gray-600">Toplam Firma</p>
                  <p className="text-2xl font-bold text-gray-900">{companies.length}</p>
                </div>
              </div>
            </div>

            <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
              <div className="flex items-center">
                <div className="p-2 bg-green-100 rounded-lg">
                  <FileText className="h-6 w-6 text-green-600" />
                </div>
                <div className="ml-4">
                  <p className="text-sm font-medium text-gray-600">Toplam Belge</p>
                  <p className="text-2xl font-bold text-gray-900">{documents.length}</p>
                </div>
              </div>
            </div>
          </>
        )}

        {(user?.role === 'CompanyAdmin' || user?.role === 'User') && (
          <>
            <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
              <div className="flex items-center">
                <div className="p-2 bg-blue-100 rounded-lg">
                  <FileText className="h-6 w-6 text-blue-600" />
                </div>
                <div className="ml-4">
                  <p className="text-sm font-medium text-gray-600">Firmadaki Belgeler</p>
                  <p className="text-2xl font-bold text-gray-900">{userCompanyDocuments.length}</p>
                </div>
              </div>
            </div>

            <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
              <div className="flex items-center">
                <div className="p-2 bg-green-100 rounded-lg">
                  <Upload className="h-6 w-6 text-green-600" />
                </div>
                <div className="ml-4">
                  <p className="text-sm font-medium text-gray-600">Yüklediğim Belgeler</p>
                  <p className="text-2xl font-bold text-gray-900">{myDocuments.length}</p>
                </div>
              </div>
            </div>
          </>
        )}

        <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
          <div className="flex items-center">
            <div className="p-2 bg-purple-100 rounded-lg">
              <FileText className="h-6 w-6 text-purple-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-600">Belge Türleri</p>
              <p className="text-2xl font-bold text-gray-900">{documentTypes.length}</p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
          <div className="flex items-center">
            <div className="p-2 bg-orange-100 rounded-lg">
              <TrendingUp className="h-6 w-6 text-orange-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-600">Bu Ay</p>
              <p className="text-2xl font-bold text-gray-900">
                {userCompanyDocuments.filter(doc => {
                  const docDate = new Date(doc.uploadDate);
                  const now = new Date();
                  return docDate.getMonth() === now.getMonth() && 
                         docDate.getFullYear() === now.getFullYear();
                }).length}
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Son Yüklenen Belgeler */}
      <div className="bg-white rounded-lg shadow-sm border border-gray-200">
        <div className="p-6 border-b border-gray-200">
          <h2 className="text-lg font-semibold text-gray-900 flex items-center">
            <Clock className="h-5 w-5 mr-2 text-gray-500" />
            Son Yüklenen Belgeler
          </h2>
        </div>
        <div className="p-6">
          {recentDocuments.length > 0 ? (
            <div className="space-y-4">
              {recentDocuments.map((doc) => (
                <div 
                  key={doc.id} 
                  className="flex items-center justify-between p-4 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors"
                >
                  <div className="flex items-center space-x-4">
                    <div className="p-2 bg-blue-100 rounded-lg">
                      <FileText className="h-5 w-5 text-blue-600" />
                    </div>
                    <div>
                      <p className="font-medium text-gray-900">{doc.originalName}</p>
                      <p className="text-sm text-gray-600">
                        {doc.documentTypeName} • {doc.uploadedByName}
                      </p>
                    </div>
                  </div>
                  <div className="text-right">
                    <p className="text-sm text-gray-900">{formatDate(doc.uploadDate)}</p>
                    <p className="text-xs text-gray-500">{doc.fileSize} MB</p>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center py-8">
              <FileText className="h-12 w-12 text-gray-400 mx-auto mb-4" />
              <p className="text-gray-600">Henüz yüklenmiş belge bulunmuyor.</p>
            </div>
          )}
        </div>
      </div>

      {/* Hızlı İşlemler */}
      <div className="bg-white rounded-lg shadow-sm border border-gray-200">
        <div className="p-6 border-b border-gray-200">
          <h2 className="text-lg font-semibold text-gray-900">Hızlı İşlemler</h2>
        </div>
        <div className="p-6">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <button className="flex items-center p-4 bg-blue-50 rounded-lg hover:bg-blue-100 transition-colors text-left">
              <Upload className="h-8 w-8 text-blue-600 mr-4" />
              <div>
                <p className="font-medium text-gray-900">Belge Yükle</p>
                <p className="text-sm text-gray-600">Yeni belge ekleyin</p>
              </div>
            </button>
            
            <button className="flex items-center p-4 bg-green-50 rounded-lg hover:bg-green-100 transition-colors text-left">
              <FileText className="h-8 w-8 text-green-600 mr-4" />
              <div>
                <p className="font-medium text-gray-900">Belgelerim</p>
                <p className="text-sm text-gray-600">Belgelerimi görüntüle</p>
              </div>
            </button>

            {(user?.role === 'SuperAdmin' || user?.role === 'CompanyAdmin') && (
              <button className="flex items-center p-4 bg-purple-50 rounded-lg hover:bg-purple-100 transition-colors text-left">
                <Users className="h-8 w-8 text-purple-600 mr-4" />
                <div>
                  <p className="font-medium text-gray-900">Kullanıcılar</p>
                  <p className="text-sm text-gray-600">Kullanıcı yönetimi</p>
                </div>
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;