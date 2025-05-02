import React, { useState } from "react";

export type DropdownItem = {
  id: number | string;
  value: string | React.ReactNode;
};

type DropdownProps = {
  items: DropdownItem[];
  onChange: (item: DropdownItem) => void;
  type?: "light" | "brown" | "custom";
  className?: string;
  placeholder?: string;
};

export default function Dropdown({
  items,
  onChange,
  type = "light",
  className = "",
  placeholder = "Select...",
}: DropdownProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [selectedItem, setSelectedItem] = useState<DropdownItem | null>(null);

  const toggleDropdown = () => setIsOpen((prev) => !prev);
  const onSelect = (item: DropdownItem) => {
    setSelectedItem(item);
    onChange(item);
    setIsOpen(false);
  };

  const typeClasses =
    type === "light"
      ? "bg-white text-black border-gray-300"
      : type === "brown"
      ? "bg-[#624935] text-white"
      : "";

  return (
    <div className={`relative inline-block w-full ${className}`}>
      <button
        onClick={toggleDropdown}
        className={`w-full border rounded-[30px] block flex flex-between px-4 py-2 ${typeClasses}`}
      >
        <span className="w-[150px] text-left">{selectedItem ? selectedItem.value : placeholder}</span>
        <span className={`${!isOpen ? "" : "rotate-180"} flex item-center transition-transform duration-300`}><img src="/assets/images/icons/dropdown_arrow.svg" /></span>
      </button>

      {isOpen && (
        <ul
          className={`absolute mt-2 w-full z-10 rounded border shadow-lg overflow-hidden bg-white ${typeClasses}`}
        >
          {items.map((item) => (
            <li
              key={item.id}
              onClick={() => onSelect(item)}
              className={`px-4 py-2 cursor-pointer hover:bg-gray-200 ${
                type === "brown" ? "hover:bg-gray-700" : ""
              }`}
            >
              {item.value}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
