function ContactMap() {
  return (
    <div className="h-[300px] sm:h-[400px] lg:h-[445px] w-full sm:w-[700px] lg:w-[1200px] 
      rounded-none sm:rounded-[150px] lg:rounded-[215px] 
      overflow-hidden border border-gray-300 
      translate-x-0 lg:translate-x-1/4 mt-8 lg:mt-0">
        <iframe
          title="Headquarters"
          src="https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d2781.997272749197!2d15.984169176508398!3d45.801651971082915!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x4765d6f1d8c155fb%3A0x88e4871cd46a6d49!2sUl.%20Vjekoslava%20Heinzela%2C%20Zagreb!5e0!3m2!1sen!2shr!4v1717168550123!5m2!1sen!2shr"
          width="100%"
          height="100%"
          style={{ border: 0 }}
          allowFullScreen
          loading="lazy"
          referrerPolicy="no-referrer-when-downgrade"
        />
      </div>
  );
}
  
  export default ContactMap;
  