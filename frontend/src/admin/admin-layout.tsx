import { Outlet } from "react-router-dom";
import HeaderAdminComponent from "./components/header-admin";


export default function AdminLayout() {
  return (
    <div className="flex">
      <div className="flex-1">
        <HeaderAdminComponent /> {/* Admin Header */}
        <main className="p-4">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
