import React, { useState } from 'react';
import { useEffect } from 'react';
import { Search as SearchIcon, FileText, Download, Eye, Calendar, User, Filter, ChevronLeft, ChevronRight } from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';
import { useApp } from '../contexts/AppContext';
import { documentService } from '../services/documentService';
import toast from 'react-hot-toast';

const Search: React.FC = () => {
  const { user } = useAuth();
  const { documentTypes, companies } = useApp();
  const [documents, setDocuments] = useState<any[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedDocumentType, setSelectedDocumentType] = useState('');
  const [selectedCompany, setSelectedCompany] = useState('');
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');
  const [showFilters, setShowFilters] = useState(false);
  const [loading, setLoading] = useState(false);
  const [searchResults, setSearchResults] = useState<any[]>([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [pageSize, setPageSize] = useState(10);

  useEffect(() => {
    if (user?.role !== 'User') {
      performSearch();
    }
  }, [user?.role]);

  // Sayfa boyutu seçenekleri
  const pageSizeOptions = [5, 10, 20, 50];

  const handlePageSizeChange = (newPageSize: number) => {
    setPageSize(newPageSize);
    setCurrentPage(1); // Reset to first page
    // Trigger search with new page size
    performSearch();
  };

  const performSearch = async () => {
    if (!user || user?.role === 'User') return;

    try {
      setLoading(true);
      const searchRequest = {
        searchTerm: searchTerm || undefined,
        documentTypeId: selectedDocumentType ? parseInt(selectedDocumentType) : undefined,
        companyId: selectedCompany ? parseInt(selectedCompany) : undefined,
        dateFrom: dateFrom || undefined,
        dateTo: dateTo || undefined,
        page: 1,
        pageSize: pageSize || 10
      };

      const result = await documentService.search(searchRequest);
      setSearchResults(result.documents);
      setTotalPages(result.totalPages);
      setTotalCount(result.totalCount);
      setPageSize(result.pageSize);
      setCurrentPage(1);
    } catch (error) {
      toast.error('Arama sırasında hata oluştu');
      setSearchResults([]);
      setTotalPages(0);
      setTotalCount(0);
    } finally {
      setLoading(false);
    }
  };

  const performPagedSearch = async (page: number) => {
    if (!user || user?.role === 'User') return;

    try {
      setLoading(true);
      const searchRequest = {
        searchTerm: searchTerm || undefined,
        documentTypeId: selectedDocumentType ? parseInt(selectedDocumentType) : undefined,
        companyId: selectedCompany ? parseInt(selectedCompany) : undefined,
        dateFrom: dateFrom || undefined,
        dateTo: dateTo || undefined,
        page: page,
        pageSize: pageSize || 10
      };

      const result = await documentService.search(searchRequest);
      setSearchResults(result.documents);
      setTotalPages(result.totalPages);
      setTotalCount(result.totalCount);
      setPageSize(result.pageSize);
      setCurrentPage(page);
    } catch (error) {
      toast.error('Arama sırasında hata oluştu');
    } finally {
      setLoading(false);
    }
  };

  // Trigger search when filters change
  useEffect(() => {
    if (user?.role !== 'User') {
      const timeoutId = setTimeout(() => {
        performSearch();
      }, 500); // Debounce search

      return () => clearTimeout(timeoutId);
    }
  }, [searchTerm, selectedDocumentType, selectedCompany, dateFrom, dateTo]);

  // Kullanıcının arama yapabileceği belgeleri filtrele
  const filteredDocuments = searchResults;

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
      const loadingToast = toast.loading(`${document.originalName} indiriliyor...`);
      const blob = await documentService.download(document.id);
      
      // Blob kontrolü
      if (!blob || blob.size === 0) {
        toast.dismiss(loadingToast);
        toast.error('Dosya bulunamadı veya boş');
        return;
      }
      
      const url = window.URL.createObjectURL(blob);
      const a = window.document.createElement('a');
      a.href = url;
      a.download = document.originalName;
      a.style.display = 'none';
      window.document.body.appendChild(a);
      a.click();
      
      // Cleanup
      window.URL.revokeObjectURL(url);
      window.document.body.removeChild(a);
      
      toast.dismiss(loadingToast);
      toast.success(`${document.originalName} indirildi`);
    } catch (error: any) {
      console.error('Download error:', error);
      const errorMessage = error.response?.data?.message || error.message || 'Dosya indirilemedi';
      toast.error(errorMessage);
    }
  };

  const handleView = async (document: any) => {
    try {
      const loadingToast = toast.loading(`${document.originalName} açılıyor...`);
      const blob = await documentService.download(document.id);
      
      // Blob kontrolü
      if (!blob || blob.size === 0) {
        toast.dismiss(loadingToast);
        toast.error('Dosya bulunamadı veya boş');
        return;
      }
      
      const url = window.URL.createObjectURL(blob);
      
      // Yeni sekmede aç
      const newWindow = window.open(url, '_blank');
      if (!newWindow) {
        // Popup blocked, fallback to download
        const a = window.document.createElement('a');
        a.href = url;
        a.download = document.originalName;
        a.style.display = 'none';
        window.document.body.appendChild(a);
        a.click();
        window.document.body.removeChild(a);
        toast.dismiss(loadingToast);
        toast.success('Dosya indirildi (popup engellendi)');
      } else {
        toast.dismiss(loadingToast);
        toast.success('Dosya yeni sekmede açıldı');
        
        // Cleanup after a delay
        setTimeout(() => {
          window.URL.revokeObjectURL(url);
        }, 1000);
      }
    } catch (error: any) {
      console.error('View error:', error);
      const errorMessage = error.response?.data?.message || error.message || 'Dosya açılamadı';
      toast.error(errorMessage);
    }
  };

  const clearFilters = () => {
    setSearchTerm('');
    setSelectedDocumentType('');
    setSelectedCompany('');
    setDateFrom('');
    setDateTo('');
    setSearchResults([]);
    setCurrentPage(1);
  };

  // Kullanıcının erişim yetkisi kontrolü
  if (user?.role === 'User') {
    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Belge Ara</h1>
          <p className="text-gray-600">Belgeler arasında arama yapın</p>
        </div>
        
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 text-center py-12">
          <SearchIcon className="h-16 w-16 text-gray-400 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-900 mb-2">
            Erişim Yetkisi Yok
          </h3>
          <p className="text-gray-600">
            Bu sayfaya erişim yetkiniz bulunmamaktadır.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Belge Ara</h1>
        <p className="text-gray-600">
          {user?.role === 'SuperAdmin' 
            ? 'Tüm belgeler arasında arama yapın'
            : 'Firma belgeleri arasında arama yapın'
          }
        </p>
      </div>

      {/* Arama ve Filtreler */}
      <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
        <div className="space-y-4">
          {/* Ana Arama */}
          <div className="relative">
            <SearchIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 h-5 w-5 text-gray-400" />
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => {
                setSearchTerm(e.target.value);
              }}
              placeholder="Belge adı, türü veya yükleyen kişi ara..."
              className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              disabled={loading}
            />
          </div>

          {/* Filtre Toggle */}
          <div className="flex items-center justify-between">
            <button
              onClick={() => setShowFilters(!showFilters)}
              className="flex items-center space-x-2 px-3 py-2 text-sm text-gray-600 hover:text-gray-900 hover:bg-gray-100 rounded-md transition-colors"
            >
              <Filter className="h-4 w-4" />
              <span>{showFilters ? 'Filtreleri Gizle' : 'Gelişmiş Filtreler'}</span>
            </button>
            
            {(selectedDocumentType || selectedCompany || dateFrom || dateTo) && (
              <button
                onClick={clearFilters}
                className="text-sm text-blue-600 hover:text-blue-800"
              >
                Filtreleri Temizle
              </button>
            )}
          </div>

          {/* Gelişmiş Filtreler */}
          {showFilters && (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 pt-4 border-t border-gray-200">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Belge Türü
                </label>
                <select
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

              {user?.role === 'SuperAdmin' && (
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Firma
                  </label>
                  <select
                    value={selectedCompany}
                    onChange={(e) => setSelectedCompany(e.target.value)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  >
                    <option value="">Tüm firmalar</option>
                    {companies.map(company => (
                      <option key={company.id} value={company.id}>
                        {company.name}
                      </option>
                    ))}
                  </select>
                </div>
              )}

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Başlangıç Tarihi
                </label>
                <input
                  type="date"
                  value={dateFrom}
                  onChange={(e) => setDateFrom(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Bitiş Tarihi
                </label>
                <input
                  type="date"
                  value={dateTo}
                  onChange={(e) => setDateTo(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                />
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Sonuçlar */}
      <div className="bg-white rounded-lg shadow-sm border border-gray-200">
        {loading ? (
          <div className="text-center py-12">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto mb-4"></div>
            <p className="text-gray-600">Aranıyor...</p>
          </div>
        ) : filteredDocuments.length > 0 ? (
          <div>
            <div className="p-6 border-b border-gray-200">
              <div className="flex justify-between items-center">
                <h2 className="text-lg font-semibold text-gray-900">
                  Arama Sonuçları ({totalCount})
                </h2>
                <div className="text-sm text-gray-600">
                  Sayfa {currentPage} / {totalPages}
                </div>
              </div>
            </div>
            <div className="divide-y divide-gray-200">
              {filteredDocuments.map((document) => (
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
                          <span className="flex items-center">
                            <User className="h-4 w-4 mr-1" />
                            {document.uploadedByName}
                          </span>
                        </div>
                        <div className="mt-2 flex items-center space-x-4 text-xs text-gray-500">
                          <span>Boyut: {formatFileSize(document.fileSize)}</span>
                          <span>Format: {document.fileExtension}</span>
                          {user?.role === 'SuperAdmin' && document.companyName && (
                            <span>Firma: {document.companyName}</span>
                          )}
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
                    </div>
                  </div>
                </div>
              ))}
            </div>
            
            {/* Sayfalama */}
            {totalPages > 1 && (
              <div className="px-6 py-4 border-t border-gray-200">
                <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
                  {/* Sayfa boyutu seçici */}
                  <div className="flex items-center space-x-2">
                    <label className="text-sm text-gray-700">Sayfa başına:</label>
                    <select
                      value={pageSize}
                      onChange={(e) => handlePageSizeChange(parseInt(e.target.value))}
                      className="px-2 py-1 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                      disabled={loading}
                    >
                      {pageSizeOptions.map(size => (
                        <option key={size} value={size}>{size}</option>
                      ))}
                    </select>
                  </div>
                  
                  <div className="text-sm text-gray-700">
                    <span className="font-medium">{((currentPage - 1) * pageSize) + 1}</span>
                    {' - '}
                    <span className="font-medium">
                      {Math.min(currentPage * pageSize, totalCount)}
                    </span>
                    {' / '}
                    <span className="font-medium">{totalCount}</span>
                    {' sonuç gösteriliyor'}
                  </div>
                  
                  <div className="flex items-center space-x-2">
                    <button
                      onClick={() => performPagedSearch(currentPage - 1)}
                      disabled={currentPage <= 1 || loading}
                      className="flex items-center px-3 py-2 text-sm font-medium text-gray-500 bg-white border border-gray-300 rounded-md hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      <ChevronLeft className="h-4 w-4 mr-1" />
                      Önceki
                    </button>
                    
                    <div className="flex items-center space-x-1">
                      {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                        let pageNum;
                        if (totalPages <= 5) {
                          pageNum = i + 1;
                        } else if (currentPage <= 3) {
                          pageNum = i + 1;
                        } else if (currentPage >= totalPages - 2) {
                          pageNum = totalPages - 4 + i;
                        } else {
                          pageNum = currentPage - 2 + i;
                        }
                        
                        return (
                          <button
                            key={pageNum}
                            onClick={() => performPagedSearch(pageNum)}
                            disabled={loading}
                            className={`px-3 py-2 text-sm font-medium rounded-md ${
                              currentPage === pageNum
                                ? 'bg-blue-600 text-white'
                                : 'text-gray-700 bg-white border border-gray-300 hover:bg-gray-50'
                            } disabled:opacity-50 disabled:cursor-not-allowed`}
                          >
                            {pageNum}
                          </button>
                        );
                      })}
                    </div>
                    
                    <button
                      onClick={() => performPagedSearch(currentPage + 1)}
                      disabled={currentPage >= totalPages || loading}
                      className="flex items-center px-3 py-2 text-sm font-medium text-gray-500 bg-white border border-gray-300 rounded-md hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      Sonraki
                      <ChevronRight className="h-4 w-4 ml-1" />
                    </button>
                  </div>
                </div>
              </div>
            )}
          </div>
        ) : (
          <div className="text-center py-12">
            <SearchIcon className="h-16 w-16 text-gray-400 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">
              {searchTerm || selectedDocumentType || selectedCompany || dateFrom || dateTo
                ? 'Sonuç bulunamadı'
                : 'Arama yapmak için yukarıdaki alana yazın'
              }
            </h3>
            <p className="text-gray-600">
              {searchTerm || selectedDocumentType || selectedCompany || dateFrom || dateTo
                ? 'Arama kriterlerinizi değiştirip tekrar deneyin.'
                : 'Belge adı, türü veya yükleyen kişi adı ile arama yapabilirsiniz.'
              }
            </p>
          </div>
        )}
      </div>
    </div>
  );
};

export default Search;