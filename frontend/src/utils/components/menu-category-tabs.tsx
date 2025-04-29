import React from "react";
import { CategoryGroup } from "../../admin/pages/products/product";


interface CategoryTab {
  menu: CategoryGroup[];
  selectedCategory: string | null;
  changeCategory: (category: string) => void;
}

export const CategoryTabs: React.FC<CategoryTab> = ({ menu, selectedCategory, changeCategory }) => {
  return (
    <div className="flex space-x-2 overflow-x-auto mb-4 customized-scrollbar">
      {menu.map((group) => (
        <button
          key={group.category}
          onClick={() => changeCategory(group.category)}
          className={`px-4 py-2 text-black font-extralight text-sm w-fit cursor-pointer ${
            selectedCategory === group.category
              ? "font-semibold text-black border-b-2"
              : "font-extralight"
          }`}
        >
          {group.category}
        </button>
      ))}
    </div>
  );
};
