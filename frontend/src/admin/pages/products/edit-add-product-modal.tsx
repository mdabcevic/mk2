import React, { useState } from "react";
import { Category, CreateCustomProductReq } from "./product";
import { useTranslation } from "react-i18next";

interface AddProductModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSave: (data: CreateCustomProductReq) => void;
  categories: Category[];
}

const AddProductModal: React.FC<AddProductModalProps> = ({
  isOpen,
  onClose,
  onSave,
  categories,
}) => {
  const [name, setName] = useState("");
  const [volume, setVolume] = useState("");
  const [categoryId, setCategoryId] = useState<number | null>(null);
  const { t } = useTranslation("admin");

  const save = () => {
    if (name && volume && categoryId !== null) {
      onSave({ name, volume, categoryId });
      setName("");
      setVolume("");
      setCategoryId(null);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 flex items-center justify-center bg-black/10 z-50 bg-opacity-10">
      <div className="bg-white p-6 rounded-lg w-[400px]">
        <div className="flex justify-between w-full">
          <h2 className="text-lg text-[#A3A3A3] font-semibold mb-4">
            {t("add_new_product")}
          </h2>
          <button onClick={onClose}>
            <img src="/assets/images/close.svg" width="30px" />
          </button>
        </div>

        <div className="mt-6">
          <label className="block text-sm mb-1">{t("product_name")}</label>
          <input
            type="text"
            value={name}
            onChange={(e) => setName(e.target.value)}
            className="border w-full p-3 rounded-[20px]"
          />
        </div>

        <div className="mt-6">
          <label className="block text-sm mb-1">{t("category")}</label>
          <select
            value={categoryId ?? ""}
            onChange={(e) => setCategoryId(Number(e.target.value))}
            className="border w-full p-3 rounded-[20px]"
          >
            <option value="" disabled>
              {t("select_category")}
            </option>
            {categories.map((cat) => (
              <option key={cat.id} value={cat.id}>
                {cat.name}
              </option>
            ))}
          </select>
        </div>

        <div className="mt-6 max-w-[150px]">
          <label className="block text-sm mb-1">{t("volume")}</label>
          <input
            type="text"
            value={volume}
            onChange={(e) => setVolume(e.target.value)}
            className="border w-full p-3 rounded-[20px]"
          />
        </div>

        <div className="flex justify-start gap-2 mt-6">
          <button
            onClick={save}
            className="px-4 py-[12px] rounded-[16px] text-white w-[150px]"
            style={{ backgroundColor: "#624935" }}
          >
            {t("save")}
          </button>
        </div>
      </div>
    </div>
  );
};

export default AddProductModal;
