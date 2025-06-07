import { BrowserRouter as Router, Routes, Route, Outlet, Navigate } from "react-router-dom";
import Places from "../../pages/places/places.tsx";
import ContactUs from "../../pages/contact-us/contactus.tsx";
import PlaceDetails from "../../pages/places/place-details/place-details.tsx";
import Layout from "../../containers/layout";
import {Menu} from "../../pages/places/menu/menu.tsx";
import { AppPaths } from "./routes";
import { lazy, Suspense } from "react";
import { authService } from "../auth/auth.service";
import { UserRole } from "../constants";
import LoginPage from "../auth/login";
import RedirectPage from "../redirect-page.tsx";
import PlaceTablesViewPublic from "../../pages/places/place-details/place-tables-view.tsx";
import Subscription from "../../pages/subscription/subscription.tsx";
import { NotificationScreen } from "../../admin/pages/table-view/notifications.tsx";
import ManagementView from "../../admin/pages/management/management.tsx";
import HomePage from "../../pages/home/home.tsx";
import MyOrders from "../../pages/places/place-details/myOrders/my-orders.tsx";

const AdminLayout = lazy(() => import("../../admin/containers/admin-layout"));
const Dashboard = lazy(() => import("../../admin/pages/dashboard/dashboard.tsx"));
const ProductsViewPage = lazy(() => import("../../admin/pages/products/products"));
const TablesPage = lazy(() => import("../../admin/pages/table-view/tables.tsx"));


function AdminRouteGuard() {
  return ((authService.userRole() == UserRole.admin || authService.userRole() == UserRole.manager || authService.userRole() === UserRole.staff) 
          && authService.tokenValid()) ? 
          <Outlet /> : 
          <Navigate to={AppPaths.public.login} replace />;
}

function AppRoutes(){
  return (
    <Router>
      <Routes>
      
        {/* Public routes */}
        <Route path={AppPaths.public.home} element={<Layout />}>
          <Route index element={<HomePage />} />
          <Route path={AppPaths.public.contactUs} element={<ContactUs />} />
          <Route path={AppPaths.public.places} element={<Places />} />
          <Route path={AppPaths.public.placeDetails} element={<PlaceDetails />} />
          <Route path={AppPaths.public.placeTables} element={<PlaceTablesViewPublic />} />
          <Route path={AppPaths.public.menu} element={<Menu />} />
          <Route path={AppPaths.public.login} element={<LoginPage />} />
          <Route path={AppPaths.public.subsciption} element={<Subscription />} />
          <Route path={AppPaths.public.myOrders} element={<MyOrders />} />
          {/* after scanning qr code redirect here */}
          <Route path={AppPaths.public.redirectPage} element={<RedirectPage />} />
          <Route path="*" element={<Places />} />  
        </Route>

        {/* Admin Routes (Using AdminLayout) */}
        <Route element={<AdminRouteGuard />}>
          <Route
              path={AppPaths.admin.dashboard}
              element={
                <Suspense fallback={<div>Loading...</div>}>
                  <AdminLayout />
                </Suspense>
              }
            >
              <Route index element={<Dashboard />} />
              <Route path={AppPaths.admin.management} element={<ManagementView />} />
              <Route path={AppPaths.admin.products} element={<ProductsViewPage />} />
              <Route path={AppPaths.admin.tables} element={<TablesPage />} />
              <Route path={AppPaths.admin.notifications} element={<NotificationScreen />} />
              <Route path="*" element={<Dashboard />} />
            </Route>
          </Route>
          
      </Routes>
    </Router>
  );
};

export default AppRoutes;