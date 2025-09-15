import React, { useState } from 'react';
import { FileType, Plus, Edit, Trash2 } from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';
import { useApp } from '../contexts/AppContext';
import Modal from '../components/Common/Modal';
import toast from 'react-hot-toast';

const DocumentTypes: React.FC = () => {
  const { user } = useAuth();
  const { documentTypes, addDocumentType, updateDocumentType, deleteDocumentType } = useApp();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingDocumentType, setEditingDocumentType] = useState<any>(null);
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    allowedExtensions: [] as string[],
    maxFileSize: 5,
    companyId: user?.companyId || ''
  });

  // Kullanıcının görebileceği belge türlerini filtrele
  const filteredDocumentTypes = user?.role === 'SuperAdmin' 
    ? documentTypes 
    : documentTypes.filter(dt => dt.companyId === user?.companyId);

  const commonExtensions = [
    { value: '.pdf', label: 'PDF' },
    { value: '.doc', label: 'Word (DOC)' },
    { value: '.docx', label: 'Word (DOCX)' },
    { value: '.xls', label: 'Excel (XLS)' },
    { value: '.xlsx', label: 'Excel (XLSX)' },
    { value: '.jpg', label: 'JPEG' },
    { value: '.jpeg', label: 'JPEG' },
    { value: '.png', label: 'PNG' },
    { value: '.txt', label: 'Text' }
  ];

  const handleOpenModal = (documentType?: any) => {
    if (documentType) {
      setEditingDocumentType(documentType);
      setFormData({
        name: documentType.name,
        description: documentType.description,
        allowedExtensions: documentType.allowedExtensions,
        maxFileSize: documentType.maxFileSize,
        companyId: documentType.companyId || user?.companyId || ''
      });
    } else {
      setEditingDocumentType(null);
      setFormData({
        name: '',
        description: '',
        allowedExtensions: [],
        maxFileSize: 5,
        companyId: user?.companyId || ''
      });
    }
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingDocumentType(null);
    setFormData({
      name: '',
      description: '',
      allowedExtensions: [],
      maxFileSize: 5,
      companyId: user?.companyId || ''
    });
  };

  const handleExtensionChange = (extension: string, checked: boolean) => {
    if (checked) {
      setFormData({
        ...formData,
        allowedExtensions: [...formData.allowedExtensions, extension]
      });
    } else {
      setFormData({
        ...formData,
        allowedExtensions: formData.allowedExtensions.filter(ext => ext !== extension)
      });
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!formData.name || !formData.description) {
      toast.error('Zorunlu alanları doldurun');
      return;
    }

    if (formData.allowedExtensions.length === 0) {
      toast.error('En az bir dosya uzantısı seçin');
      return;
    }

    if (formData.maxFileSize <= 0) {
      toast.error('Maksimum dosya boyutu 0\'dan büyük olmalıdır');
      return;
    }

    if (editingDocumentType) {
      updateDocumentType(editingDocumentType.id, formData);
      toast.success('Belge türü başarıyla güncellendi');
    } else {
      addDocumentType(formData);
      toast.success('Belge türü başarıyla eklendi');
    }
    
    handleCloseModal();
  };

  const handleDelete = (documentType: any) => {
    if (window.confirm(`${documentType.name} belge türünü silmek istediğinizden emin misiniz?`)) {
      deleteDocumentType(documentType.id);
      toast.success('Belge türü başarıyla silindi');
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Belge Türleri</h1>
          <p className="text-gray-600">
            {user?.role === 'SuperAdmin' 
              ? 'Tüm belge türlerini yönetin'
              : 'Firma belge türlerini yönetin'
            }
          </p>
        </div>
        <button
          onClick={() => handleOpenModal()}
          className="flex items-center space-x-2 px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
        >
          <Plus className="h-4 w-4" />
          <span>Yeni Belge Türü</span>
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {filteredDocumentTypes.map((documentType) => (
          <div key={documentType.id} className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 hover:shadow-md transition-shadow">
            <div className="flex items-start justify-between mb-4">
              <div className="p-2 bg-blue-100 rounded-lg">
                <FileType className="h-6 w-6 text-blue-600" />
              </div>
              <div className="flex items-center space-x-2">
                <button
                  onClick={() => handleOpenModal(documentType)}
                  className="p-2 text-blue-600 hover:bg-blue-100 rounded-lg transition-colors"
                  title="Düzenle"
                >
                  <Edit className="h-4 w-4" />
                </button>
                <button
                  onClick={() => handleDelete(documentType)}
                  className="p-2 text-red-600 hover:bg-red-100 rounded-lg transition-colors"
                  title="Sil"
                >
                  <Trash2 className="h-4 w-4" />
                </button>
              </div>
            </div>
            
            <h3 className="text-lg font-semibold text-gray-900 mb-2">
              {documentType.name}
            </h3>
            <p className="text-gray-600 text-sm mb-4">
              {documentType.description}
            </p>
            
            <div className="space-y-2 text-sm">
              <div>
                <span className="font-medium text-gray-700">İzin verilen formatlar:</span>
                <div className="flex flex-wrap gap-1 mt-1">
                  {documentType.allowedExtensions.map((ext: string) => (
                    <span key={ext} className="px-2 py-1 bg-gray-100 text-gray-700 rounded text-xs">
                      {ext}
                    </span>
                  ))}
                </div>
              </div>
              <div>
                <span className="font-medium text-gray-700">Maksimum boyut:</span>
                <span className="ml-2 text-gray-600">{documentType.maxFileSize} MB</span>
              </div>
            </div>
          </div>
        ))}
      </div>

      {filteredDocumentTypes.length === 0 && (
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 text-center py-12">
          <FileType className="h-16 w-16 text-gray-400 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-900 mb-2">
            Henüz belge türü bulunmuyor
          </h3>
          <p className="text-gray-600 mb-4">
            İlk belge türünü eklemek için butona tıklayın.
          </p>
          <button
            onClick={() => handleOpenModal()}
            className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
          >
            İlk Belge Türünü Ekle
          </button>
        </div>
      )}

      {/* Modal */}
      <Modal
        isOpen={isModalOpen}
        onClose={handleCloseModal}
        title={editingDocumentType ? 'Belge Türü Düzenle' : 'Yeni Belge Türü Ekle'}
        maxWidth="lg"
      >
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Belge Türü Adı *
            </label>
            <input
              type="text"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="Örn: TC Kimlik, Özlük Hakları"
              required
            />
          </div>
          
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Açıklama *
            </label>
            <textarea
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              rows={3}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="Belge türü hakkında açıklama yazın"
              required
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              İzin Verilen Dosya Formatları *
            </label>
            <div className="grid grid-cols-2 md:grid-cols-3 gap-2">
              {commonExtensions.map((ext) => (
                <label key={ext.value} className="flex items-center space-x-2">
                  <input
                    type="checkbox"
                    checked={formData.allowedExtensions.includes(ext.value)}
                    onChange={(e) => handleExtensionChange(ext.value, e.target.checked)}
                    className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                  />
                  <span className="text-sm text-gray-700">{ext.label}</span>
                </label>
              ))}
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Maksimum Dosya Boyutu (MB) *
            </label>
            <input
              type="number"
              min="1"
              max="100"
              value={formData.maxFileSize}
              onChange={(e) => setFormData({ ...formData, maxFileSize: parseInt(e.target.value) || 5 })}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              required
            />
            <p className="text-xs text-gray-500 mt-1">
              1 MB ile 100 MB arasında bir değer girin
            </p>
          </div>

          <div className="flex justify-end space-x-3 pt-4">
            <button
              type="button"
              onClick={handleCloseModal}
              className="px-4 py-2 text-gray-700 bg-gray-100 rounded-md hover:bg-gray-200 transition-colors"
            >
              İptal
            </button>
            <button
              type="submit"
              className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
            >
              {editingDocumentType ? 'Güncelle' : 'Ekle'}
            </button>
          </div>
        </form>
      </Modal>
    </div>
  );
};

export default DocumentTypes;