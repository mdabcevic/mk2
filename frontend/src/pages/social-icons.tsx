import { JSX } from "react";
import { FaInstagram, FaLinkedinIn, FaFacebookF } from "react-icons/fa";

function SocialIcons() {
  return (
    <div className="flex items-center gap-6 mt-10">
      <SocialLink icon={<FaInstagram />} href="#" />
      <SocialLink icon={<FaLinkedinIn />} href="#" />
      <SocialLink icon={<FaFacebookF />} href="#" />
    </div>
  );
}

export default SocialIcons;

type SocialLinkProps = {
  icon: JSX.Element;
  href: string;
};

function SocialLink({ icon, href }: SocialLinkProps) {
  return (
    <a
      href={href}
      target="_blank"
      rel="noopener noreferrer"
      className="text-white hover:text-gray-300 transition-colors text-xl"
    >
      {icon}
    </a>
  );
}
