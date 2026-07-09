import React, { useState } from 'react';
import { Download, FileText, Image as ImageIcon, FileSpreadsheet, Loader2, CheckCircle, AlertTriangle } from 'lucide-react';
import html2canvas from 'html2canvas';

interface ExportPanelProps {
  onExportPdf: () => Promise<void>;
  onExportExcel: () => Promise<void>;
  targetRef?: React.RefObject<HTMLElement>;
}

export const ExportPanel: React.FC<ExportPanelProps> = ({ onExportPdf, onExportExcel, targetRef }) => {
  const [loadingType, setLoadingType] = useState<string | null>(null);
  const [statusMsg, setStatusMsg] = useState<{ type: 'success' | 'error', text: string } | null>(null);

  const handleExportPng = async () => {
    if (!targetRef?.current) {
      setStatusMsg({ type: 'error', text: 'Chưa xác định vùng cần chụp.' });
      return;
    }
    
    setLoadingType('png');
    setStatusMsg(null);
    try {
      const canvas = await html2canvas(targetRef.current, {
        scale: 2,
        useCORS: true,
        backgroundColor: '#ffffff'
      });
      const dataUrl = canvas.toDataURL('image/png');
      const link = document.createElement('a');
      link.download = `GPA_Summary_${new Date().toISOString().split('T')[0]}.png`;
      link.href = dataUrl;
      link.click();
      setStatusMsg({ type: 'success', text: 'Đã xuất ảnh PNG thành công.' });
    } catch (err) {
      setStatusMsg({ type: 'error', text: 'Lỗi khi chụp màn hình.' });
    } finally {
      setLoadingType(null);
      setTimeout(() => setStatusMsg(null), 3000);
    }
  };

  const handleExportPdf = async () => {
    setLoadingType('pdf');
    setStatusMsg(null);
    try {
      await onExportPdf();
      setStatusMsg({ type: 'success', text: 'Đã xuất PDF thành công.' });
    } catch (err: any) {
      setStatusMsg({ type: 'error', text: err.message || 'Lỗi xuất PDF.' });
    } finally {
      setLoadingType(null);
      setTimeout(() => setStatusMsg(null), 3000);
    }
  };

  const handleExportExcel = async () => {
    setLoadingType('excel');
    setStatusMsg(null);
    try {
      await onExportExcel();
      setStatusMsg({ type: 'success', text: 'Đã xuất Excel thành công.' });
    } catch (err: any) {
      setStatusMsg({ type: 'error', text: err.message || 'Lỗi xuất Excel.' });
    } finally {
      setLoadingType(null);
      setTimeout(() => setStatusMsg(null), 3000);
    }
  };

  return (
    <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-6 shadow-sm">
      <h3 className="text-lg font-extrabold text-gray-900 dark:text-white flex items-center gap-2 mb-4">
        <Download className="h-5 w-5 text-indigo-500" />
        Xuất dữ liệu (Export)
      </h3>
      
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
        <button
          onClick={handleExportPdf}
          disabled={loadingType !== null}
          className="flex flex-col items-center justify-center p-4 border border-gray-200 dark:border-gray-800 rounded-xl hover:border-red-400 hover:bg-red-50 dark:hover:bg-red-950/20 transition-all disabled:opacity-50"
        >
          {loadingType === 'pdf' ? <Loader2 className="h-6 w-6 text-red-500 animate-spin mb-2" /> : <FileText className="h-6 w-6 text-red-500 mb-2" />}
          <span className="text-sm font-semibold text-gray-800 dark:text-gray-200">Xuất PDF</span>
        </button>
        
        <button
          onClick={handleExportExcel}
          disabled={loadingType !== null}
          className="flex flex-col items-center justify-center p-4 border border-gray-200 dark:border-gray-800 rounded-xl hover:border-green-400 hover:bg-green-50 dark:hover:bg-green-950/20 transition-all disabled:opacity-50"
        >
          {loadingType === 'excel' ? <Loader2 className="h-6 w-6 text-green-600 animate-spin mb-2" /> : <FileSpreadsheet className="h-6 w-6 text-green-600 mb-2" />}
          <span className="text-sm font-semibold text-gray-800 dark:text-gray-200">Xuất Excel</span>
        </button>

        <button
          onClick={handleExportPng}
          disabled={loadingType !== null}
          className="flex flex-col items-center justify-center p-4 border border-gray-200 dark:border-gray-800 rounded-xl hover:border-blue-400 hover:bg-blue-50 dark:hover:bg-blue-950/20 transition-all disabled:opacity-50"
        >
          {loadingType === 'png' ? <Loader2 className="h-6 w-6 text-blue-500 animate-spin mb-2" /> : <ImageIcon className="h-6 w-6 text-blue-500 mb-2" />}
          <span className="text-sm font-semibold text-gray-800 dark:text-gray-200">Lưu ảnh PNG</span>
        </button>
      </div>

      {statusMsg && (
        <div className={`mt-4 p-3 rounded-xl flex items-center gap-2 text-sm ${
          statusMsg.type === 'success' ? 'bg-emerald-50 text-emerald-700 border border-emerald-200' : 'bg-red-50 text-red-700 border border-red-200'
        }`}>
          {statusMsg.type === 'success' ? <CheckCircle className="h-4 w-4" /> : <AlertTriangle className="h-4 w-4" />}
          {statusMsg.text}
        </div>
      )}
    </div>
  );
};
