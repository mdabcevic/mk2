import { useRef, useState } from 'react';

import Products from '../products/products';

import { authService } from '../../../utils/auth/auth.service';
import OrdersTable from '../table-view/orders-table';
import { MenuTable } from '../products/menu-table';

const tabs = ['Menu', 'Products', 'Orders'];

export default function ManagementView() {
  const [activeTab, setActiveTab] = useState('Menu');
  const menuRef = useRef<any>(null);
  const productsRef = useRef<any>(null);
  const renderTabContent = () => {
    switch (activeTab) {
      case 'Menu':
        return <MenuTable ref={menuRef} placeId={authService.placeId()} />;
      case 'Products':
        return <Products ref={productsRef} />;
      case 'Orders':
        return <OrdersTable rerender={0} showStatus={false}/>;
      default:
        return null;
    }
  };
  const handleAddClick = () => {
    if (activeTab === 'Menu' && menuRef.current?.openAddModal) {
      menuRef.current.openAddModal();
    } else if (activeTab === 'Products' && productsRef.current?.openAddModal) {
      productsRef.current.openAddModal();
    }
  };
  return (
    <div className="pt-[120px]">
      <div className="flex justify-end mb-2">
        <button
          onClick={handleAddClick}
          className={`px-4 py-2 rounded text-white ${activeTab === "Orders" ? "invisible" : "visible"}`}
          style={{ backgroundColor: "#624935" }}
        >
          {activeTab === "Menu"
            ? "+ Add Item"
            : activeTab === "Products"
            ? "+ Add Product"
            : "Orders"}
        </button>
      </div>
      <div className=" border-b">
        <div className='w-full max-w-[1500px] flex space-x-6 pl-10'>
            {tabs.map((tab) => (
            <button
                key={tab}
                onClick={() => setActiveTab(tab)}
                className={`pb-2 text-base px-10 font-medium border-b-2 transition-colors ${
                activeTab === tab
                    ? 'text-[#7E5E44] border-[#7E5E44] font-bold'
                    : 'text-[#737373] border-transparent'
                }`}
            >
                {tab}
            </button>
            ))}
        </div>  
      </div>

      <div className="mt-6 w-full max-w-[1500px]">
        {renderTabContent()}
      </div>
    </div>
  );
}
