import { useTranslation } from "react-i18next";
import LoginForm from "./login-form";
import loginIllustration from "../../../public/assets/images/coffee_shop.jpg"


const LoginPage = () => {
  useTranslation("public");

  return (
    <div
      className="min-h-[80vh] flex items-center justify-center px-4"
      style={{
        background: "",
      }}
    >
      {/* main container */}
      <div className="min-h-screen flex bg-[#F5F0EA]">
      {/* Left: Illustration */}
      <div className="hidden lg:flex w-1/2 items-center justify-center">
        <div className="bg-white border border-[#d1c3aa] rounded-xl shadow-lg h-[85%] w-[90%] flex items-center justify-center p-4">
          <img
            src={loginIllustration}
            alt="Login visual"
            className="object-contain max-h-full"
          />
        </div>
      </div>

      {/* Right: Login Card */}
      <div className="w-full lg:w-1/2 flex items-center justify-center px-6">
        <div className="bg-[#E9DDC9] border border-[#b6a584] rounded-2xl shadow-lg p-8 w-full max-w-md">
          <LoginForm />
        </div>
      </div>
    </div>


        {/* Not implemented yet. It will be added in the future! */}
        {/* <p className="text-sm text-center text-gray-600 mt-4">
          Don’t have an account?{" "}
          <span className="text-[#f49241] hover:underline cursor-pointer">Sign up</span>
        </p> */}
      </div>
  );
};

export default LoginPage;
