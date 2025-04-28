import { Outlet } from "react-router-dom";
import HeaderAdminComponent from "./header-admin";


export default function AdminLayout() {
  return (
    <div className="flex">
      <div className="flex-1">
        <HeaderAdminComponent />
        <main className="">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
