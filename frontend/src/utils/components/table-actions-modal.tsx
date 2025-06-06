import React from "react";
import { motion } from "framer-motion";
import { TableStatusString } from "../constants";
import { useTranslation } from "react-i18next";


interface Props {
  tableLabel: string;
  isDisabled:boolean;
  onClose: () => void;
  onSetStatus: (status: TableStatusString) => void;
  onGenerateQR?: () => void;
  disable: (tableLabel: string) => void;
  enable:(tableLabel: string) => void;
}

const TableActionModal: React.FC<Props> = ({
  tableLabel,
  isDisabled,
  onClose,
  onSetStatus,
  onGenerateQR,
  disable,
  enable,
}) => {
  const { t } = useTranslation("public");
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
          className={`${isDisabled ? "hidden" : "block"} w-full text-left mb-2 hover:text-mocha-600 transition-colors duration-100`}
        >
          {t("generate_qr")}
        </button>
      )}
      <button
        onClick={() => {
          onSetStatus(TableStatusString.empty);
          onClose();
        }}
        className={`${isDisabled ? "hidden" : "block"} w-full text-left mb-2 hover:text-mocha-600 transition-colors duration-100`}
      >
        {t("set_as_empty")}
      </button>
      <button
        onClick={() => {
          onSetStatus(TableStatusString.occupied);
          onClose();
        }}
        className={`${isDisabled ? "hidden" : "block"} w-full text-left mb-2 hover:text-mocha-600 transition-colors duration-100`}
      >
        {t("set_as_occupied")}
      </button>
      <button
        onClick={() => {
          onSetStatus(TableStatusString.reserved);
          onClose();
        }}
        className={`${isDisabled ? "hidden" : "block"} w-full text-left mb-2 hover:text-mocha-600 transition-colors duration-100`}
      >
        {t("set_as_reserved")}
      </button>
      <button
        onClick={() => {
          disable(tableLabel);
          onClose();
        }}
        className={`${isDisabled ? "hidden" : "block"} w-full text-left mb-2 hover:text-mocha-600 transition-colors duration-100`}
      >
        {t("disable")}
      </button>
      <button
        onClick={() => {
          enable(tableLabel);
          onClose();
        }}
        className={`${!isDisabled ? "hidden" : "block"} w-full text-left mb-2 hover:text-mocha-600 transition-colors duration-100`}
      >
        {t("enable")}
      </button>
      <button onClick={onClose} className=" absolute right-5 top-3 z-10">
            <img src="/assets/images/icons/close_icon.svg" alt="close" />
          </button>
    </motion.div>
  );
};

export default TableActionModal;
