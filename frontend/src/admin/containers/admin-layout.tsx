import { Outlet } from "react-router-dom";
import HeaderAdminComponent from "./header-admin";


export default function AdminLayout() {
  return (
    <div className="flex transition-all duration-1000 ease-in-out">
      <div className="flex-1 bg-white transition-all duration-1000 ease-in-out">
        <HeaderAdminComponent />
        <main className="">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
