import { Outlet } from "react-router-dom";
import HeaderComponent from "./header";
import { authService } from "../utils/auth/auth.service";
import { UserRole } from "../utils/constants";
import Footer from "./footer";

const Layout = () => {
  const userRole = authService.userRole();
  return (
    <>
      <HeaderComponent/>
      <main>
        <Outlet />
      </main>
       
      
    </>
  );
};

export default Layout;
