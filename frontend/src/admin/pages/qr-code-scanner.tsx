import { useState } from "react";
import { Scanner } from "@yudiel/react-qr-scanner";

const QrCodeScanner = () => {
  const [scanResult, setScanResult] = useState("");
  const [qrCodeValue, setQrCodeValue] = useState("");

  return (
    <div className="flex flex-col items-center p-4 bg-white shadow-lg rounded-xl w-80">
      <h2 className="text-xl font-bold">Scan QR</h2>
      <Scanner
        onScan={(result:any) => {
            let qrValue = result[0].rawValue; 
            console.log(qrValue);
            setQrCodeValue(qrValue);
            setScanResult(qrValue === "kiki" ? "Success" : "Ticket not valid");
        }}
        onError={(error) => console.error("QR Scan Error:", error)}
        constraints={{ facingMode: "environment" }}
      />
      <p className="mt-4 text-lg font-semibold">{scanResult}</p>
      <p className="mt-4 text-lg font-semibold">{qrCodeValue}</p>
    </div>
  );
};

export default QrCodeScanner;
