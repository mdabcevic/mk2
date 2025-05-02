function ContactForm() {
    return (
      <form className="flex flex-col gap-4 max-w-[460px] w-full pl-10">
        {/* ...form fields... */}
        <label>
              <span className="block mb-1">Email</span>
              <input
                type="email"
                className="w-full px-4 py-2 border border-gray-400 rounded"
                placeholder="you@example.com"
              />
            </label>

            <label>
              <span className="block mb-1">Subject</span>
              <input
                type="text"
                className="w-full px-4 py-2 border border-gray-400 rounded"
                placeholder="Subject"
              />
            </label>

            <label>
              <span className="block mb-1">Message</span>
              <textarea
                className="w-full px-4 py-2 border border-gray-400 rounded h-40 resize-none"
                placeholder="Your message..."
              />
            </label>

            <button
              type="submit"
              className="bg-[#432B1F] text-white px-6 py-2 rounded w-fit mt-2"
            >
              SEND
            </button>
      </form>
    );
  }
  
  export default ContactForm;