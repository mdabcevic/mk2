import { Outlet } from "react-router-dom";
import HeaderComponent from "./header";
import { useEffect, useRef, useState } from "react";
import { authService } from "../utils/auth/auth.service";
import { UserRole } from "../utils/constants";
import Footer from "./footer";

const mainPaddingTop = 100;
const Layout = () => {
  const userRole = authService.userRole();
  return (
    <>
      <HeaderComponent/>
      <main style={{ paddingTop: `${userRole === UserRole.guest ? 0 : mainPaddingTop}px` }}>
        <Outlet /> {/* This renders the matched route component */}
      </main>
      <Footer />
    </>
  );
};

export default Layout;
