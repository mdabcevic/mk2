import React, { useState } from "react";
import { useTranslation } from "react-i18next";
import { authService } from "./auth.service";
import { AppPaths } from "../routing/routes";
import { Button } from "../components/button";

const LoginPage = () => {
  const { t } = useTranslation("public");
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");

  const login = async (e: React.FormEvent) => {
    e.preventDefault();
    await authService.login(username,password);
    window.location.href = AppPaths.admin.dashboard;
  };

  return (
    <div
      className="min-h-[80vh] flex items-center justify-center px-4"
      style={{
        background: "",
      }}
    >
      <div className="w-full max-w-sm bg-white p-6 rounded-2xl shadow-md">
        <h2 className="text-2xl font-bold text-center text-gray-800 mb-6">
          {t("login")}
        </h2>
        <form onSubmit={login} className="space-y-4 " >
          <div>
            <label htmlFor="username" className="block text-sm font-medium text-gray-700">
            {t("username")}
            </label>
            <input
              type="text"
              id="username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              className="mt-1 w-full border border-gray-300 text-gray-700 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-orange-300"
              required
            />
          </div>

          <div>
            <label htmlFor="password" className="block text-sm font-medium text-gray-700">
            {t("password")}
            </label>
            <input
              type="password"
              id="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="mt-1 w-full border border-gray-300 text-gray-700 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-orange-300"
              required
            />
          </div>

          <Button textValue={t("login")} type="brown" size="large" onClick={() => login} className="w-full" />
        </form>


        {/* Not implemented yet. It will be added in the future! */}
        {/* <p className="text-sm text-center text-gray-600 mt-4">
          Don’t have an account?{" "}
          <span className="text-[#f49241] hover:underline cursor-pointer">Sign up</span>
        </p> */}
      </div>
    </div>
  );
};

export default LoginPage;
