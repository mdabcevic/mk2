import React, { useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";

export type DropdownItem<T = string> = {
  id: number | string;
  value: T;
  label?: string | React.ReactNode;
};

type DropdownProps<T = string> = {
  items: DropdownItem<T>[];
  onChange: (item: DropdownItem<T>) => void;
  value?: T;
  defaultValue?: T;
  type?: "light" | "brown" | "custom";
  className?: string;
  buttonClassName?: string;
  menuClassName?: string;
  itemClassName?: string;
};

export default function Dropdown<T = string>({
  items,
  onChange,
  value,
  defaultValue,
  type = "light",
  className = "",
  buttonClassName = "",
  menuClassName = "",
  itemClassName = "",
}: DropdownProps<T>) {
  const [isOpen, setIsOpen] = useState(false);
  const [internalValue, setInternalValue] = useState<T | undefined>(defaultValue);
  const ref = useRef<HTMLDivElement>(null);
  const { t } = useTranslation("admin");

  const selectedItem = items.find((item) => item.value === (value ?? internalValue)) ?? null;
  const toggleDropdown = () => setIsOpen((prev) => !prev);

  const handleSelect = (item: DropdownItem<T>) => {
    if (value === undefined) setInternalValue(item.value);
    onChange(item);
    setIsOpen(false);
  };

  const typeClasses = type === "light" ? "bg-white text-black border-gray-300": 
                      type === "brown" ? "bg-[#624935] text-white" : 
                      type === "custom" ? "" : "";

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) {
        setIsOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  return (
    <div ref={ref} className={`relative inline-block ${className}`}>
      <button onClick={toggleDropdown} className={`w-full border rounded-[30px] flex justify-between items-center px-4 py-2 ${typeClasses} ${buttonClassName}`} >
        <span>
          {
            typeof selectedItem?.label === "string" || typeof selectedItem?.label === "number"
              ? selectedItem.label : selectedItem?.label ?? String(selectedItem?.value ?? `${t("placeholder")}...` )
          }
        </span>
        <img src="/assets/images/icons/dropdown_arrow.svg" className={`mr-4 transition-transform duration-300 ${isOpen ? "rotate-180" : ""}`}
        />
      </button>

      {isOpen && (
        <ul className={`absolute mt-2 w-full z-10 rounded border shadow-lg bg-white overflow-hidden ${menuClassName}`}>
          {items.map((item) => (
            <li key={item.id} onClick={() => handleSelect(item)} className={`px-4 py-2 cursor-pointer hover:bg-gray-200 ${itemClassName}`}>
              {(typeof selectedItem?.label === "string" || typeof selectedItem?.label === "number") ? item.label : String(item?.value ?? "")}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
