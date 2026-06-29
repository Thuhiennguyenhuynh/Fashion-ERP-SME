import React, { useEffect, useRef } from 'react';
import { Html5QrcodeScanner } from 'html5-qrcode';

interface WebcamScannerProps {
  onScanSuccess: (barcode: string) => void;
}

const WebcamScanner: React.FC<WebcamScannerProps> = ({ onScanSuccess }) => {
  const scannerRef = useRef<Html5QrcodeScanner | null>(null);

  useEffect(() => {
    scannerRef.current = new Html5QrcodeScanner(
      "barcode-reader",
      { fps: 10, qrbox: { width: 250, height: 100 } },
      false
    );

    scannerRef.current.render(
      (decodedText) => {
        // Chỉ gọi hàm đưa data ra ngoài. 
        // Không dùng pause() nữa vì Modal sẽ tự đóng ngay lập tức.
        onScanSuccess(decodedText);
      },
      (errorMessage) => {
        const errorStr = String(errorMessage);
        // Bỏ qua các lỗi rác không cần thiết
        if (errorStr.includes("NotFoundException") || errorStr.includes("Cannot pause")) {
          return; 
        }
        console.warn("Lỗi quét:", errorStr);
      }
    );

    return () => {
      // Bọc try-catch để chặn lỗi "removeChild" khi React và thư viện cùng tranh nhau xóa DOM
      try {
        if (scannerRef.current) {
          scannerRef.current.clear().catch(() => {
            // Bỏ qua lỗi promise nội bộ của thư viện
          });
        }
      } catch (error) {
        // Âm thầm bỏ qua
      }
    };
  }, [onScanSuccess]);

  return (
    <div 
      id="barcode-reader" 
      style={{ width: '100%', maxWidth: '400px', margin: '0 auto' }} 
    />
  );
};

export default WebcamScanner;