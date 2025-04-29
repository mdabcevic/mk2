import React from "react";
import { motion } from "framer-motion";
import { TableStatusString } from "../constants";


interface Props {
  tableLabel: string;
  onClose: () => void;
  onSetStatus: (status: TableStatusString) => void;
  onGenerateQR?: () => void;
}

const TableActionModal: React.FC<Props> = ({
  tableLabel,
  onClose,
  onSetStatus,
  onGenerateQR,
}) => {
  return (
    <motion.div
      className="absolute z-50 text-sm rounded-[40px] shadow p-3 bg-white text-brown-500 border w-[200px]"
      style={{ top: "-110px", left: "50%", transform: "translateX(-50%)" }}
      onClick={(e) => e.stopPropagation()}
      initial={{ opacity: 0, scale: 0.95 }}
      animate={{ opacity: 1, scale: 1 }}
      transition={{ duration: 0.2 }}
    >
      {onGenerateQR && (
        <button
          onClick={() => {
            onGenerateQR();
            onClose();
          }}
          className="block w-full text-left mb-2 hover:text-mocha-600 transition-colors duration-100"
        >
          Generate QR
        </button>
      )}
      <button
        onClick={() => {
          onSetStatus(TableStatusString.empty);
          onClose();
        }}
        className="block w-full text-left mb-2 hover:text-mocha-600 transition-colors duration-100"
      >
        Set as Empty
      </button>
      <button
        onClick={() => {
          onSetStatus(TableStatusString.occupied);
          onClose();
        }}
        className="block w-full text-left mb-2 hover:text-mocha-600 transition-colors duration-100"
      >
        Set as Occupied
      </button>
      <button
        onClick={() => {
          onSetStatus(TableStatusString.reserved);
          onClose();
        }}
        className="block w-full text-left mb-2 hover:text-mocha-600 transition-colors duration-100"
      >
        Set as Reserved
      </button>
      <button onClick={onClose} className=" absolute right-5 top-3">
            <img src="/assets/images/icons/close_icon.svg" alt="close" />
          </button>
    </motion.div>
  );
};

export default TableActionModal;
