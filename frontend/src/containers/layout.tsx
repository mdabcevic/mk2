import { Outlet } from "react-router-dom";
import HeaderComponent from "./header";
import { authService } from "../utils/auth/auth.service";
import { UserRole } from "../utils/constants";
import Footer from "./footer";

const mainPaddingTop = 100;
const Layout = () => {
  const userRole = authService.userRole();
  return (
    <>
      <HeaderComponent/>
      <main style={{ paddingTop: `${mainPaddingTop}px` }}>
        <Outlet />
      </main>
      {userRole !== UserRole.guest && userRole !== UserRole.manager && userRole !== UserRole.admin && <Footer />} 
      
    </>
  );
};

export default Layout;
