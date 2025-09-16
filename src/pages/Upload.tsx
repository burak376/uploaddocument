import React, { useState } from 'react';
import { useEffect } from 'react';
import { Upload as UploadIcon, File, X, CheckCircle } from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';
import { useApp } from '../contexts/AppContext';
import { documentTypeService } from '../services/documentTypeService';
import { documentService } from '../services/documentService';
import toast from 'react-hot-toast';

const Upload: React.FC = () => {
  const { user } = useAuth();
  const { documentTypes, setDocumentTypes, addDocument } = useApp();
  const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
  const [selectedDocumentType, setSelectedDocumentType] = useState('');
  const [isUploading, setIsUploading] = useState(false);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadDocumentTypes();
  }, []);

  const loadDocumentTypes = async () => {
    try {
      setLoading(true);
      const data = await documentTypeService.getAll();
      setDocumentTypes(data);
    } catch (error) {
      toast.error('Belge türleri yüklenirken hata oluştu');
    } finally {
      setLoading(false);
    }
  };

  const userDocumentTypes = documentTypes.filter(dt => 
    !dt.companyId || dt.companyId === user?.companyId
  );

  const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const files = event.target.files;
    if (files) {
      const newFiles = Array.from(files);
      setSelectedFiles(prev => [...prev, ...newFiles]);
    }
  };

  const removeFile = (index: number) => {
    setSelectedFiles(prev => prev.filter((_, i) => i !== index));
  };

  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const validateFile = (file: File, documentType: any) => {
    // Dosya uzantısı kontrolü
    const fileExtension = '.' + file.name.split('.').pop()?.toLowerCase();
    if (!documentType.allowedExtensions.includes(fileExtension)) {
      return `${file.name} dosyası için geçersiz uzantı. İzin verilen: ${documentType.allowedExtensions.join(', ')}`;
    }

    // Dosya boyutu kontrolü
    const fileSizeMB = file.size / (1024 * 1024);
    if (fileSizeMB > documentType.maxFileSize) {
      return `${file.name} dosyası çok büyük. Maksimum: ${documentType.maxFileSize} MB`;
    }

    return null;
  };

  const handleUpload = async () => {
    if (!selectedDocumentType) {
      toast.error('Lütfen belge türü seçin');
      return;
    }

    if (selectedFiles.length === 0) {
      toast.error('Lütfen en az bir dosya seçin');
      return;
    }

    const selectedType = userDocumentTypes.find(dt => dt.id === parseInt(selectedDocumentType));
    if (!selectedType) {
      toast.error('Geçersiz belge türü');
      return;
    }

    // Dosya validasyonu
    for (const file of selectedFiles) {
      const error = validateFile(file, selectedType);
      if (error) {
        toast.error(error);
        return;
      }
    }

    setIsUploading(true);

    try {
      for (const file of selectedFiles) {
        const uploadedDocument = await documentService.upload(file, selectedType.id);
        addDocument(uploadedDocument);
      }

      toast.success(`${selectedFiles.length} dosya başarıyla yüklendi`);
      setSelectedFiles([]);
      setSelectedDocumentType('');
      
    } catch (error) {
      toast.error('Dosya yükleme işlemi başarısız');
    } finally {
      setIsUploading(false);
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Belge Yükle</h1>
        <p className="text-gray-600">
          Yeni belgelerinizi yükleyin
        </p>
      </div>

      <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
        <div className="space-y-6">
          {/* Belge Türü Seçimi */}
          <div>
            <label htmlFor="documentType" className="block text-sm font-medium text-gray-700 mb-2">
              Belge Türü *
            </label>
            <select
              id="documentType"
              value={selectedDocumentType}
              onChange={(e) => setSelectedDocumentType(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              disabled={isUploading}
            >
              <option value="">Belge türü seçin</option>
              {userDocumentTypes.map(dt => (
                <option key={dt.id} value={dt.id}>
                  {dt.name} ({dt.allowedExtensions.join(', ')}) - Max: {dt.maxFileSize} MB
                </option>
              ))}
            </select>
          </div>

          {/* Belge Türü Detayları */}
          {selectedDocumentType && (
            <div className="bg-blue-50 border border-blue-200 rounded-md p-4">
              <h3 className="font-medium text-blue-900 mb-2">Seçili Belge Türü Bilgileri</h3>
              {(() => {
                const selectedType = userDocumentTypes.find(dt => dt.id === selectedDocumentType);
                return selectedType && (
                  <div className="text-sm text-blue-800 space-y-1">
                    <p><span className="font-medium">İsim:</span> {selectedType.name}</p>
                    <p><span className="font-medium">Açıklama:</span> {selectedType.description}</p>
                    <p><span className="font-medium">İzin verilen formatlar:</span> {selectedType.allowedExtensions.join(', ')}</p>
                    <p><span className="font-medium">Maksimum dosya boyutu:</span> {selectedType.maxFileSize} MB</p>
                  </div>
                );
              })()}
            </div>
          )}

          {/* Dosya Seçme Alanı */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Dosyalar *
            </label>
            <div className="border-2 border-dashed border-gray-300 rounded-lg p-6 text-center hover:border-gray-400 transition-colors">
              <UploadIcon className="h-12 w-12 text-gray-400 mx-auto mb-4" />
              <div className="space-y-2">
                <label htmlFor="fileInput" className="cursor-pointer">
                  <span className="text-blue-600 hover:text-blue-500 font-medium">
                    Dosya seçmek için tıklayın
                  </span>
                  <input
                    id="fileInput"
                    type="file"
                    multiple
                    onChange={handleFileSelect}
                    className="hidden"
                    disabled={isUploading || !selectedDocumentType}
                  />
                </label>
                <p className="text-sm text-gray-500">
                  veya dosyaları buraya sürükleyin
                </p>
              </div>
            </div>
          </div>

          {/* Seçili Dosyalar */}
          {selectedFiles.length > 0 && (
            <div>
              <h3 className="text-sm font-medium text-gray-700 mb-3">
                Seçili Dosyalar ({selectedFiles.length})
              </h3>
              <div className="space-y-2">
                {selectedFiles.map((file, index) => (
                  <div key={index} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                    <div className="flex items-center space-x-3">
                      <File className="h-5 w-5 text-gray-400" />
                      <div>
                        <p className="text-sm font-medium text-gray-900">{file.name}</p>
                        <p className="text-xs text-gray-500">{formatFileSize(file.size)}</p>
                      </div>
                    </div>
                    <button
                      onClick={() => removeFile(index)}
                      className="p-1 hover:bg-gray-200 rounded-full"
                      disabled={isUploading}
                    >
                      <X className="h-4 w-4 text-gray-500" />
                    </button>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Yükleme Butonu */}
          <div className="flex justify-end">
            <button
              onClick={handleUpload}
              disabled={isUploading || selectedFiles.length === 0 || !selectedDocumentType}
              className="flex items-center space-x-2 px-6 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              {isUploading ? (
                <>
                  <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                  <span>Yükleniyor...</span>
                </>
              ) : (
                <>
                  <CheckCircle className="h-4 w-4" />
                  <span>Yükle</span>
                </>
              )}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Upload;