
function SocialIcons() {
  return (
    <div className={`flex items-center gap-6 mt-8 sm:mt-10 justify-center lg:justify-start`}>
      <a href="#" target="_blank" rel="noopener noreferrer" className="text-white hover:text-gray-300 transition-colors text-xl">
        <img src="/assets/images/ig.svg"  alt="instagram" />
      </a>
      <a href="#" target="_blank" rel="noopener noreferrer" className="text-white hover:text-gray-300 transition-colors text-xl">
        <img src="/assets/images/ln.svg"  alt="linkedin" />
      </a>
      <a href="#" target="_blank" rel="noopener noreferrer" className="text-white hover:text-gray-300 transition-colors text-xl">
        <img src="/assets/images/facebook.svg"  alt="facebook" />
      </a>
    </div>
  );
}

export default SocialIcons;
