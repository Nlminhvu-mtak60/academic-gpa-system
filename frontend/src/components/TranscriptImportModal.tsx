import React, { useState, useRef } from 'react';
import { Upload, X, FileText, CheckCircle, AlertTriangle, Image as ImageIcon, FileSpreadsheet } from 'lucide-react';
import { useLanguage } from '../contexts/LanguageContext';

export interface ImportedCoursePreview {
  courseName: string;
  credits: number;
  finalScore: number | null;
  componentScores: Record<string, number>;
  confidence: number;
}

export interface TranscriptImportPreviewResult {
  courses: ImportedCoursePreview[];
  detectedUniversity: string;
  sourceType: string;
  warnings: string[];
}

interface TranscriptImportModalProps {
  isOpen: boolean;
  onClose: () => void;
  semesterId: string;
  onConfirm: (data: any) => Promise<void>;
  // For the actual API call, we might pass a prop or call a service directly
  onParse: (file: File, sourceType: string) => Promise<TranscriptImportPreviewResult | null>;
}

export const TranscriptImportModal: React.FC<TranscriptImportModalProps> = ({
  isOpen,
  onClose,
  semesterId,
  onConfirm,
  onParse
}) => {
  const { t } = useLanguage();
  const fileInputRef = useRef<HTMLInputElement>(null);
  
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [sourceType, setSourceType] = useState<string>('excel');
  const [loading, setLoading] = useState(false);
  const [previewData, setPreviewData] = useState<TranscriptImportPreviewResult | null>(null);
  const [error, setError] = useState<string | null>(null);

  if (!isOpen) return null;

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      const file = e.target.files[0];
      setSelectedFile(file);
      setError(null);
      
      // Auto-detect type based on extension
      const ext = file.name.split('.').pop()?.toLowerCase();
      if (ext === 'pdf') setSourceType('pdf');
      else if (['xlsx', 'xls', 'csv'].includes(ext || '')) setSourceType('excel');
      else if (['jpg', 'jpeg', 'png'].includes(ext || '')) setSourceType('imageocr');
      else setSourceType('text');
    }
  };

  const handleUploadAndParse = async () => {
    if (!selectedFile) return;
    
    setLoading(true);
    setError(null);
    try {
      const result = await onParse(selectedFile, sourceType);
      if (result) {
        setPreviewData(result);
      } else {
        setError("Failed to parse the file.");
      }
    } catch (err: any) {
      setError(err.message || "An error occurred during parsing.");
    } finally {
      setLoading(false);
    }
  };

  const handleConfirm = async () => {
    if (!previewData) return;
    setLoading(true);
    try {
      const cleanCourses = previewData.courses.map(c => {
        const cleanedComponentScores: Record<string, number> = {};
        if (c.componentScores) {
          Object.keys(c.componentScores).forEach(key => {
            const val = c.componentScores[key];
            if (val !== null && val !== undefined && val !== '' as any && !isNaN(Number(val))) {
              cleanedComponentScores[key] = Number(val);
            }
          });
        }
        return {
          ...c,
          courseName: c.courseName || '',
          credits: Number(c.credits) || 0,
          finalScore: (c.finalScore !== null && c.finalScore !== undefined && c.finalScore !== '' as any) ? Number(c.finalScore) : null,
          componentScores: cleanedComponentScores,
          confidence: Number(c.confidence) || 1
        };
      });

      await onConfirm({
        sourceType,
        courses: cleanCourses
      });
      onClose();
    } catch (err: any) {
      setError(err.message || "Failed to confirm import.");
    } finally {
      setLoading(false);
    }
  };

  const resetState = () => {
    setSelectedFile(null);
    setPreviewData(null);
    setError(null);
    if (fileInputRef.current) fileInputRef.current.value = '';
  };

  const handleCourseChange = (index: number, field: string, value: string, componentKey?: string) => {
    if (!previewData) return;
    const newCourses = [...previewData.courses];
    
    if (componentKey) {
      newCourses[index] = {
        ...newCourses[index],
        componentScores: {
          ...newCourses[index].componentScores,
          [componentKey]: value === '' ? null : Number(value)
        }
      };
    } else {
      newCourses[index] = {
        ...newCourses[index],
        [field]: value === '' ? null : (field === 'courseName' ? value : Number(value))
      };
    }
    
    setPreviewData({
      ...previewData,
      courses: newCourses
    });
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm transition-opacity">
      <div className="w-full max-w-4xl bg-white dark:bg-gray-900 rounded-2xl shadow-xl border border-gray-100 dark:border-gray-800 overflow-hidden flex flex-col max-h-[90vh]">
        
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100 dark:border-gray-800 shrink-0">
          <h3 className="text-xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
            <Upload className="h-6 w-6 text-indigo-500" />
            Nhập Bảng Điểm (Import Transcript)
          </h3>
          <button
            onClick={onClose}
            className="p-1.5 rounded-lg text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
          >
            <X className="h-6 w-6" />
          </button>
        </div>

        {/* Body */}
        <div className="flex-1 overflow-y-auto p-6 space-y-6">
          {error && (
            <div className="p-4 bg-red-50 border border-red-200 rounded-xl text-red-700 flex items-start gap-3">
              <AlertTriangle className="h-5 w-5 shrink-0" />
              <div className="text-sm">{error}</div>
            </div>
          )}

          {!previewData ? (
            // Upload State
            <div className="space-y-6">
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                <button 
                  onClick={() => setSourceType('excel')}
                  className={`p-4 border rounded-xl flex flex-col items-center gap-2 transition-all ${sourceType === 'excel' ? 'border-indigo-500 bg-indigo-50 dark:bg-indigo-900/20 text-indigo-700 dark:text-indigo-400 ring-2 ring-indigo-500/20' : 'border-gray-200 dark:border-gray-700 hover:border-indigo-300 dark:text-gray-300'}`}
                >
                  <FileSpreadsheet className="h-8 w-8" />
                  <span className="text-sm font-semibold">Excel / CSV</span>
                </button>
                <button 
                  onClick={() => setSourceType('pdf')}
                  className={`p-4 border rounded-xl flex flex-col items-center gap-2 transition-all ${sourceType === 'pdf' ? 'border-indigo-500 bg-indigo-50 dark:bg-indigo-900/20 text-indigo-700 dark:text-indigo-400 ring-2 ring-indigo-500/20' : 'border-gray-200 dark:border-gray-700 hover:border-indigo-300 dark:text-gray-300'}`}
                >
                  <FileText className="h-8 w-8" />
                  <span className="text-sm font-semibold">PDF Text</span>
                </button>
                <button 
                  onClick={() => setSourceType('imageocr')}
                  className={`p-4 border rounded-xl flex flex-col items-center gap-2 transition-all ${sourceType === 'imageocr' ? 'border-indigo-500 bg-indigo-50 dark:bg-indigo-900/20 text-indigo-700 dark:text-indigo-400 ring-2 ring-indigo-500/20' : 'border-gray-200 dark:border-gray-700 hover:border-indigo-300 dark:text-gray-300'}`}
                >
                  <ImageIcon className="h-8 w-8" />
                  <span className="text-sm font-semibold">Image OCR</span>
                </button>
                <button 
                  onClick={() => setSourceType('text')}
                  className={`p-4 border rounded-xl flex flex-col items-center gap-2 transition-all ${sourceType === 'text' ? 'border-indigo-500 bg-indigo-50 dark:bg-indigo-900/20 text-indigo-700 dark:text-indigo-400 ring-2 ring-indigo-500/20' : 'border-gray-200 dark:border-gray-700 hover:border-indigo-300 dark:text-gray-300'}`}
                >
                  <FileText className="h-8 w-8" />
                  <span className="text-sm font-semibold">Raw Text</span>
                </button>
              </div>

              <div 
                className="border-2 border-dashed border-gray-300 dark:border-gray-700 rounded-2xl p-10 flex flex-col items-center justify-center text-center cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors"
                onClick={() => fileInputRef.current?.click()}
              >
                <Upload className="h-12 w-12 text-gray-400 mb-4" />
                <h4 className="text-lg font-bold text-gray-900 dark:text-white">
                  {selectedFile ? selectedFile.name : 'Nhấn để chọn file'}
                </h4>
                <p className="text-sm text-gray-500 mt-2">
                  Hỗ trợ: .xlsx, .csv, .pdf, .png, .jpg, .txt
                </p>
                <input 
                  type="file" 
                  ref={fileInputRef} 
                  className="hidden" 
                  onChange={handleFileChange}
                />
              </div>

              <div className="flex justify-end">
                <button
                  onClick={handleUploadAndParse}
                  disabled={!selectedFile || loading}
                  className="px-6 py-2.5 bg-indigo-600 hover:bg-indigo-700 disabled:bg-indigo-400 text-white font-bold rounded-xl shadow transition-colors flex items-center gap-2"
                >
                  {loading ? 'Đang phân tích...' : 'Phân tích file'}
                </button>
              </div>
            </div>
          ) : (
            // Preview State
            <div className="space-y-6">
              <div className="flex items-center justify-between bg-indigo-50 dark:bg-indigo-900/20 p-4 rounded-xl border border-indigo-100 dark:border-indigo-800/30">
                <div>
                  <h4 className="font-bold text-indigo-900 dark:text-indigo-300">
                    Trường phát hiện: {previewData.detectedUniversity}
                  </h4>
                  <p className="text-sm text-indigo-700 dark:text-indigo-400">
                    Phương thức: {previewData.sourceType} | Số môn: {previewData.courses.length}
                  </p>
                </div>
                <button 
                  onClick={resetState}
                  className="px-4 py-2 bg-white dark:bg-gray-800 text-indigo-600 dark:text-indigo-400 rounded-lg text-sm font-semibold border border-indigo-200 dark:border-indigo-700 hover:bg-indigo-50 dark:hover:bg-gray-700 transition-colors"
                >
                  Chọn file khác
                </button>
              </div>

              {previewData.warnings && previewData.warnings.length > 0 && (
                <div className="p-4 bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-800/40 rounded-xl">
                  <h5 className="font-bold text-amber-800 dark:text-amber-400 flex items-center gap-2 mb-2">
                    <AlertTriangle className="h-4 w-4" /> Cảnh báo
                  </h5>
                  <ul className="list-disc list-inside text-sm text-amber-700 dark:text-amber-300 space-y-1">
                    {previewData.warnings.map((w, i) => <li key={i}>{w}</li>)}
                  </ul>
                </div>
              )}

              <div className="border border-gray-200 dark:border-gray-800 rounded-xl overflow-hidden">
                <table className="w-full text-left border-collapse">
                  <thead>
                    <tr className="bg-gray-50 dark:bg-gray-800/50 text-xs font-bold text-gray-500 dark:text-gray-400 uppercase tracking-wider border-b border-gray-200 dark:border-gray-800">
                      <th className="px-4 py-3">Tên môn học</th>
                      <th className="px-4 py-3 text-center">Tín chỉ</th>
                      
                      {previewData.courses.length > 0 && Array.from(new Set(previewData.courses.flatMap(c => Object.keys(c.componentScores || {})))).map(key => (
                        <th key={key} className="px-4 py-3 text-center">{key}</th>
                      ))}
                      
                      <th className="px-4 py-3 text-center">Điểm TK</th>
                      <th className="px-4 py-3 text-center">Độ tin cậy</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100 dark:divide-gray-800 text-sm dark:text-gray-300">
                    {previewData.courses.map((c, i) => {
                      const allCompKeys = Array.from(new Set(previewData.courses.flatMap(c => Object.keys(c.componentScores || {}))));
                      return (
                        <tr key={i} className={c.confidence < 0.8 ? 'bg-red-50/50 dark:bg-red-900/10' : ''}>
                        <td className="px-4 py-3 font-semibold">
                          <input 
                            type="text" 
                            value={c.courseName} 
                            onChange={(e) => handleCourseChange(i, 'courseName', e.target.value)}
                            className="w-full bg-transparent border-b border-transparent hover:border-gray-300 focus:border-indigo-500 focus:outline-none"
                          />
                        </td>
                        <td className="px-4 py-3 text-center">
                          <input 
                            type="number" 
                            value={c.credits} 
                            onChange={(e) => handleCourseChange(i, 'credits', e.target.value)}
                            className="w-16 text-center bg-transparent border-b border-transparent hover:border-gray-300 focus:border-indigo-500 focus:outline-none"
                          />
                        </td>
                        
                        {allCompKeys.map(key => (
                           <td key={key} className="px-4 py-3 text-center text-gray-500">
                             <input 
                                type="number" 
                                step="0.1"
                                value={c.componentScores && c.componentScores[key] !== undefined && c.componentScores[key] !== null ? c.componentScores[key] : ''} 
                                onChange={(e) => handleCourseChange(i, 'componentScores', e.target.value, key)}
                                className="w-16 text-center bg-transparent border-b border-transparent hover:border-gray-300 focus:border-indigo-500 focus:outline-none"
                             />
                           </td>
                        ))}
                        
                        <td className="px-4 py-3 text-center font-bold">
                          <input 
                            type="number" 
                            step="0.1"
                            value={c.finalScore !== null ? c.finalScore : ''} 
                            onChange={(e) => handleCourseChange(i, 'finalScore', e.target.value)}
                            className="w-16 text-center bg-transparent border-b border-transparent hover:border-gray-300 focus:border-indigo-500 focus:outline-none font-bold text-gray-900 dark:text-white"
                          />
                        </td>
                        <td className="px-4 py-3 text-center">
                          <span className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-bold ${
                            c.confidence >= 0.9 ? 'bg-emerald-100 text-emerald-800' :
                            c.confidence >= 0.8 ? 'bg-amber-100 text-amber-800' :
                            'bg-red-100 text-red-800 border border-red-200'
                          }`}>
                            {(c.confidence * 100).toFixed(0)}%
                          </span>
                        </td>
                      </tr>
                    );
                  })}
                  </tbody>
                </table>
              </div>

            </div>
          )}
        </div>

        {/* Footer */}
        {previewData && (
          <div className="flex items-center justify-end gap-3 px-6 py-4 border-t border-gray-100 dark:border-gray-800 bg-gray-50 dark:bg-gray-900/50 shrink-0">
            <button
              onClick={onClose}
              className="px-5 py-2.5 text-sm font-semibold text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-800 rounded-xl transition-colors border border-gray-200 dark:border-gray-700"
            >
              Hủy
            </button>
            <button
              onClick={handleConfirm}
              disabled={loading || previewData.courses.length === 0}
              className="px-5 py-2.5 text-sm font-bold text-white bg-indigo-600 hover:bg-indigo-700 disabled:bg-indigo-400 rounded-xl transition-colors shadow flex items-center gap-2"
            >
              {loading ? 'Đang lưu...' : (
                <>
                  <CheckCircle className="h-5 w-5" /> Xác nhận lưu vào Database
                </>
              )}
            </button>
          </div>
        )}
      </div>
    </div>
  );
};
