import React from 'react';
import { Clock, RotateCcw, FileText, CheckCircle2 } from 'lucide-react';

interface ImportBatch {
  id: string;
  importedAt: string;
  sourceType: string;
  courseCount: number;
}

interface ImportHistoryPanelProps {
  batches: ImportBatch[];
  onUndo: (batchId: string) => void;
}

export const ImportHistoryPanel: React.FC<ImportHistoryPanelProps> = ({ batches, onUndo }) => {
  if (!batches || batches.length === 0) return null;

  return (
    <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-2xl p-5 shadow-sm mt-6">
      <h3 className="text-base font-bold text-gray-900 dark:text-white flex items-center gap-2 mb-4">
        <Clock className="h-5 w-5 text-brand-500" />
        Lịch sử Nhập điểm (Import History)
      </h3>
      
      <div className="space-y-3">
        {batches.map(batch => (
          <div key={batch.id} className="flex items-center justify-between p-3 border border-gray-100 dark:border-gray-800 rounded-xl bg-gray-50/50 dark:bg-gray-800/50">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-brand-100 dark:bg-brand-900/40 text-brand-600 dark:text-brand-400 rounded-lg">
                <FileText className="h-4 w-4" />
              </div>
              <div>
                <p className="text-sm font-semibold text-gray-800 dark:text-gray-200">
                  {batch.sourceType}
                </p>
                <div className="flex items-center gap-2 text-xs text-gray-500">
                  <span>{new Date(batch.importedAt).toLocaleString('vi-VN')}</span>
                  <span>•</span>
                  <span className="flex items-center gap-1 text-green-600 dark:text-green-400">
                    <CheckCircle2 className="h-3 w-3" />
                    {batch.courseCount} môn
                  </span>
                </div>
              </div>
            </div>
            
            <button
              onClick={() => {
                if (window.confirm('Bạn có chắc muốn hoàn tác (undo) lần nhập này? Tất cả môn học trong lần nhập này sẽ bị xóa.')) {
                  onUndo(batch.id);
                }
              }}
              className="p-2 text-gray-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-950/30 rounded-lg transition-colors tooltip-trigger relative"
              title="Hoàn tác (Undo)"
            >
              <RotateCcw className="h-4 w-4" />
            </button>
          </div>
        ))}
      </div>
    </div>
  );
};
