import { JSX } from "react";
import { FaInstagram, FaLinkedinIn, FaFacebookF } from "react-icons/fa";

function SocialIcons({ className = "" }: SocialIconsProps) {
  return (
    <div className={`flex items-center gap-6 mt-10 ${className}`}>
      <SocialLink icon={<FaInstagram />} href="#" />
      <SocialLink icon={<FaLinkedinIn />} href="#" />
      <SocialLink icon={<FaFacebookF />} href="#" />
    </div>
  );
}

type SocialIconsProps = {
  readonly className?: string;
};

export default SocialIcons;

type SocialLinkProps = {
  readonly icon: JSX.Element;
  readonly href: string;
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
