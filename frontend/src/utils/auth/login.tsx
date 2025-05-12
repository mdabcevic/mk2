import { useTranslation } from "react-i18next";
import LoginForm from "./login-form";
import loginIllustration from "../../../public/assets/images/coffee_shop.png"
import CurveSvg from "../../pages/contact-us/curve-svg";


const LoginPage = () => {
  useTranslation("public");

  return (
    <div className="min-h-screen flex items-center justify-center bg-[#F5F0EA] px-4 py-8">
      {/* Main Card */}
      <div className="flex w-full max-w-6xl min-h-[600px] rounded-2xl shadow-lg overflow-hidden border border-[#d9cbb2] bg-white relative">
        {/* Left: Illustration */}
        <div className="relative w-1/2 hidden lg:block">
          <img
            src={loginIllustration}
            alt="Cafe illustration"
            className="w-full h-full object-cover"
          />
          {/* Optional SVG overlay */}
          <div className="absolute top-0 right-0 h-full w-20 z-10">
            <CurveSvg />
          </div>
        </div>

        {/* Right: Form Section */}
        <div className="w-full lg:w-1/2 bg-[#E9DDC9] px-10 py-12 flex items-center justify-center">
          <div className="w-full max-w-md">
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
