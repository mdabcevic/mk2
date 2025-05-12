import React, { useState } from "react";
import { useTranslation } from "react-i18next";
import { authService } from "../auth/auth.service";
import { AppPaths } from "../routing/routes";
import { Button } from "../components/button";

const LoginForm = () => {
  const { t } = useTranslation("public");
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");

  const login = async (e: React.FormEvent) => {
    e.preventDefault();
    await authService.login(username, password);
    window.location.href = AppPaths.admin.dashboard;
  };

  return (
    <div>
      <h2 className="text-2xl font-bold text-center text-gray-800 mb-6">
        {t("login")}
      </h2>
      <form onSubmit={login} className="space-y-4 ">
        <div>
          <label
            htmlFor="username"
            className="block text-sm font-medium text-gray-700"
          >
            {t("username")}
          </label>
          <input
            type="text"
            id="username"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            className="mt-1 w-full border border-[#A87C4F] text-gray-700 rounded-lg px-3 py-2 focus:outline-none focus:ring-1 focus:ring-[#A87C4F]"
            required
          />
        </div>

        <div>
          <label
            htmlFor="password"
            className="block text-sm font-medium text-gray-700"
          >
            {t("password")}
          </label>
          <input
            type="password"
            id="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            className="mt-1 w-full border border-[#A87C4F] text-gray-700 rounded-lg px-3 py-2 focus:outline-none focus:ring-1 focus:ring-[#A87C4F]"
            required
          />
        </div>

        <Button
          textValue={t("login")}
          type="brown"
          size="large"
          onClick={() => login}
          className="w-full"
        />
      </form>
    </div>
  );
};

export default LoginForm;
